using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class BreathingManager : MonoBehaviour
{
    public static BreathingManager instance;

    [Header("Breathing Settings")]
    [Tooltip("Timer in seconds")]
    public float inhaleTimer;
    public float pauseTimer, exhaleTimer;
    public float targetDuration = 30f;
    public float delayAfterCompletingExercise = 2f;

    [Header("UI: ")]
    public float uiFadeDuration = 2f;
    public Transform uiRef;
    public Image[] breathingUIArray;
    public Vector3[] pathPoints;

    [Header("Debug/Refernces: ")]
    [SerializeField] bool beginOnStart = false;
    [SerializeField] bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;

    public TMP_Text debugText;

    //Events
    public event Action onBreathingFinishedEvent;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        debugText.gameObject.SetActive(false);
        debugText.text = "";

        if (beginOnStart)
            StartCoroutine(BreathingExcerciseCoroutine());
    }

    public void BeginBreathingExerciseTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExerciseTutorial;
        StartCoroutine(BreathingExcerciseCoroutine());
    }

    IEnumerator BreathingExcerciseCoroutine()
    {
        breathingInProgress = true;
        breathingTimer = 0f;

        //Debug Text
        debugText.gameObject.SetActive(true);
        debugText.text = "";

        do
        {
            //Inhale
            inhale = true;
            debugText.text = "inhale";
            debugText.transform.DOScale(1.5f, inhaleTimer);
            //uiRef.DOMoveX(5.9f, inhaleTimer / 4);

            uiRef.DOLocalPath(pathPoints, inhaleTimer, PathType.Linear, PathMode.Ignore);

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            inhale = false;
            pause = true;
            debugText.text = "pause";


            yield return new WaitForSeconds(pauseTimer);

            //Exhale
            pause = false;
            exhale = true;
            debugText.text = "exhale";
            debugText.transform.DOScale(1f, exhaleTimer);

            uiRef.DOLocalPath(pathPoints, exhaleTimer, PathType.Linear, PathMode.Ignore);

            yield return new WaitForSeconds(exhaleTimer);

            exhale = false;
        }
        while (breathingTimer < targetDuration);

        debugText.text = "Complete!";
        breathingInProgress = false;

        UpdateBreathingUIState(0f);

        yield return new WaitForSeconds(delayAfterCompletingExercise);

        debugText.gameObject.SetActive(false);
        onBreathingFinishedEvent?.Invoke();

    }

    /// <summary>
    /// This function is used to fade the breathing UI in or out.
    /// 1 = Fade in
    /// 0 = Fade out
    /// </summary>
    /// <param name="alpha"></param>
    public void UpdateBreathingUIState(float alpha)
    {
        foreach(Image image in breathingUIArray)
        {
            image.DOFade(alpha, uiFadeDuration);
        }
    }

    private void Update()
    {
        if (breathingInProgress)
            breathingTimer += Time.deltaTime;
    }
}

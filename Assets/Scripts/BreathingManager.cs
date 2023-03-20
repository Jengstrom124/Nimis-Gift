using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class BreathingManager : MonoBehaviour
{
    public static BreathingManager instance;

    [Header("Breathing Settings")]
    [Tooltip("Timer in seconds")]
    public float inhaleTimer;
    public float pauseTimer, exhaleTimer;
    public float targetDuration = 30f;
    public float delayAfterCompletingExercise = 2f;

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

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            inhale = false;
            pause = true;
            debugText.text = "pause";


            yield return new WaitForSeconds(pauseTimer);

            //exhale
            pause = false;
            exhale = true;
            debugText.text = "exhale";

            yield return new WaitForSeconds(exhaleTimer);

            exhale = false;
        }
        while (breathingTimer < targetDuration);

        debugText.text = "Complete!";
        breathingInProgress = false;

        yield return new WaitForSeconds(delayAfterCompletingExercise);

        debugText.gameObject.SetActive(false);

        onBreathingFinishedEvent?.Invoke();

    }

    private void Update()
    {
        if (breathingInProgress)
            breathingTimer += Time.deltaTime;
    }
}

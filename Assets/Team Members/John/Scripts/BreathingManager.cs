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
    public float tutorialDuration, fullDuration;
    public float delayAfterCompletingExercise = 2f;

    [Header("UI: ")]
    public float uiFadeDuration = 2f;
    public Transform uiRef;
    public Image[] breathingUIArray;
    public Vector3[] pathPoints;

    [Header("Audio: ")]
    public AudioSource breathingAudioSource;
    public AudioClip inhaleAudio, exhaleAudio;

    [Header("Debug/Refernces: ")]
    [SerializeField] bool beginOnStart = false;
    [SerializeField] bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;
    bool inTutorial = false;

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
        inTutorial = true;
        targetDuration = tutorialDuration;
    }

    public void BeginBreathingExercise()
    {
        UpdateBreathingUIState(1);

        if(!breathingTimersUpdated)
        {
            inhaleTimer = 3f;
            exhaleTimer = 3f;
            pauseTimer = 3f;
            targetDuration = fullDuration;
        }

        StartCoroutine(BreathingExcerciseCoroutine());
    }

    IEnumerator BreathingExcerciseCoroutine()
    {
        NimiExperienceManager.instance.canInteractWithTree = false;
        breathingInProgress = true;
        breathingTimer = 0f;

        //Debug Text
        debugText.gameObject.SetActive(true);
        debugText.text = "";

        yield return new WaitForSeconds(1f);

        do
        {
            breathingAudioSource.Stop();

            //Inhale
            inhale = true;
            debugText.text = "Inhale";
            debugText.transform.DOScale(1.5f, inhaleTimer);
            breathingAudioSource.clip = inhaleAudio;
            breathingAudioSource.Play();
            //uiRef.DOMoveX(5.9f, inhaleTimer / 4);

            //uiRef.DOLocalPath(pathPoints, inhaleTimer, PathType.Linear, PathMode.Ignore);
            uiRef.DOLocalMoveX(2.35f, inhaleTimer);

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            inhale = false;
            pause = true;
            debugText.text = "Hold";

            uiRef.DOLocalMoveY(-2.05f, pauseTimer);


            yield return new WaitForSeconds(pauseTimer);

            breathingAudioSource.Stop();

            //Exhale
            pause = false;
            exhale = true;
            debugText.text = "Exhale";
            debugText.transform.DOScale(1f, exhaleTimer);
            breathingAudioSource.clip = exhaleAudio;
            breathingAudioSource.Play();

            //uiRef.DOLocalPath(pathPoints, exhaleTimer, PathType.Linear, PathMode.Ignore);
            uiRef.DOLocalMoveX(0, exhaleTimer);

            yield return new WaitForSeconds(exhaleTimer);

            //Pause
            inhale = false;
            pause = true;
            exhale = false;

            debugText.text = "Hold";
            uiRef.DOLocalMoveY(0, pauseTimer);

            yield return new WaitForSeconds(pauseTimer);
        }
        while (breathingTimer < targetDuration);

        debugText.text = "Complete!";
        breathingInProgress = false;
        if (inTutorial)
            inTutorial = false;

        UpdateBreathingUIState(0f);

        yield return new WaitForSeconds(delayAfterCompletingExercise);

        debugText.gameObject.SetActive(false);
        onBreathingFinishedEvent?.Invoke();
        NimiExperienceManager.instance.canInteractWithTree = true;
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

    bool breathingTimersUpdated = false;
    private void Update()
    {
        if (breathingInProgress)
        {
            breathingTimer += Time.deltaTime;

            if(!inTutorial)
            {
                if (!breathingTimersUpdated)
                {
                    if (breathingTimer > targetDuration / 2)
                    {
                        inhaleTimer = 4f;
                        exhaleTimer = 4f;
                        pauseTimer = 4f;
                        breathingTimersUpdated = true;
                    }
                }
            }
        }
    }
}

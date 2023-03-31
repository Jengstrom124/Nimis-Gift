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
    public float firstProgressionIncrease = 2.5f;
    public float secondProgressionIncrease = 3.25f;
    public float targetDuration = 30f;
    public float tutorialDuration, fullDuration;
    public float delayAfterCompletingExercise = 2f;

    [Header("UI: ")]
    public float uiFadeDuration = 2f;
    public Transform uiRef;
    public Image[] breathingUIArray;
    public Vector3[] pathPoints;
    public Image breathingUIBackdrop;

    [Header("Audio: ")]
    public AudioSource breathingAudioSource;
    public AudioClip inhaleAudio, exhaleAudio;

    [Header("Debug/Refernces: ")]
    [SerializeField] bool beginOnStart = false;
    [SerializeField] bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;
    //[SerializeField] float timeElapsed;
    [SerializeField] bool inTutorial = false;

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
            inhaleTimer = firstProgressionIncrease;
            exhaleTimer = firstProgressionIncrease;
            pauseTimer = firstProgressionIncrease;
            targetDuration = fullDuration;
        }

        StartCoroutine(BreathingExcerciseCoroutine());
    }

    bool wasInhale = false;
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
            //timeElapsed = 0f;
            if (breathingUIBackdrop.color.a == 1)
            {
                var tempColor = breathingUIBackdrop.color;
                tempColor.a = 0f;
                breathingUIBackdrop.color = tempColor;
            }

            //Inhale
            inhale = true;
            debugText.text = "Inhale";
            debugText.transform.DOScale(1.5f, inhaleTimer);
            breathingAudioSource.clip = inhaleAudio;
            breathingAudioSource.Play();

            uiRef.DOLocalMoveX(2.35f, inhaleTimer);
            breathingUIBackdrop.DOFade(1f, inhaleTimer);
            wasInhale = true;

            yield return new WaitForSeconds(inhaleTimer);
            
            //timeElapsed = 0f;

            //Pause
            inhale = false;
            pause = true;
            debugText.text = "Hold";

            uiRef.DOLocalMoveY(-2.05f, pauseTimer);

            yield return new WaitForSeconds(pauseTimer);

            breathingAudioSource.Stop();
            //timeElapsed = 0f;

            //Exhale
            pause = false;
            exhale = true;
            wasInhale = false;
            debugText.text = "Exhale";
            debugText.transform.DOScale(1f, exhaleTimer);
            breathingAudioSource.clip = exhaleAudio;
            breathingAudioSource.Play();

            uiRef.DOLocalMoveX(0, exhaleTimer);
            breathingUIBackdrop.DOFade(0f, inhaleTimer);

            yield return new WaitForSeconds(exhaleTimer);

            //timeElapsed = 0f;

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
    Vector3 desiredInhalePos = new Vector3(2.35f, 0, 0);
    Vector3 desiredExhalePos = new Vector3(0, -2.05f, 0);
    float xPos, yPos;
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
                        inhaleTimer = secondProgressionIncrease;
                        exhaleTimer = secondProgressionIncrease;
                        pauseTimer = secondProgressionIncrease;
                        breathingTimersUpdated = true;
                    }
                }
            }

            /*
            if(inhale)
            {
                //xPos = Mathf.Lerp(0, 2.35f, timeElapsed / inhaleTimer);
                //uiRef.position = new Vector3(xPos, 0f, 0f);
                //uiRef.position = Vector3.Lerp(uiRef.position, desiredInhalePos, timeElapsed / inhaleTimer);
                //Vector3.MoveTowards(uiRef.position, desiredInhalePos, //speed)
            }
            else if(exhale)
            {
                //xPos = Mathf.Lerp(2.35f, 0, timeElapsed / exhaleTimer);
                //uiRef.position = new Vector3(xPos, -2.05f, 0f);
                uiRef.position = Vector3.Lerp(uiRef.position, desiredExhalePos, timeElapsed / inhaleTimer);
            }
            else if(pause && wasInhale)
            {
                uiRef.position = new Vector3(2.35f, Mathf.Lerp(0, -2.05f, timeElapsed / inhaleTimer), 0f);
            }
            else
            {
                uiRef.position = new Vector3(Mathf.Lerp(-2.05f, 0, timeElapsed / inhaleTimer), 0f, 0f);
            }

            timeElapsed = Time.deltaTime;
            */
        }
    }
}

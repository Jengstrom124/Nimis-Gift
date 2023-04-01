using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
    public Animator uiAnimator;
    public float uiFadeDuration = 2f;
    public float nimiFadeDuration = 2f;
    public GameObject uiRef;
    public Image[] breathingUIArray;
    public Vector3[] pathPoints;
    public Image breathingUIBackdrop;
    public GameObject canvas;

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
    public GameObject nimi;
    public ParticleSystem postBreathingParticles;

    //Events
    public event Action onBreathingFinishedEvent;
    public event Action onBreathingStartedEvent;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        debugText.text = "";

        if (beginOnStart)
        {
            canvas.SetActive(true);
            StartCoroutine(BreathingExcerciseCoroutine());
        }
        else
            canvas.SetActive(false);
    }

    public void BeginBreathingExerciseTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExerciseTutorial;
        inTutorial = true;
        targetDuration = tutorialDuration;

        StartCoroutine(BreathingExcerciseCoroutine());
    }

    public void BeginBreathingExercise()
    {
        if(!breathingTimersUpdated)
        {
            inhaleTimer = firstProgressionIncrease;
            exhaleTimer = firstProgressionIncrease;
            pauseTimer = firstProgressionIncrease;
            targetDuration = fullDuration;
        }

        StartCoroutine(BreathingExcerciseCoroutine());
    }

    IEnumerator BreathingExcerciseCoroutine()
    {
        onBreathingStartedEvent?.Invoke();

        if (!inTutorial)
        {
            UpdateBreathingUIState(1);

            yield return new WaitForSeconds(nimiFadeDuration + 3f);
        }

        breathingInProgress = true;
        breathingTimer = 0f;

        //Debug Text
        debugText.text = "";

        //yield return new WaitForSeconds(1f);

        do
        {
            breathingAudioSource.Stop();

            if (breathingUIBackdrop.color.a == 1)
            {
                var tempColor = breathingUIBackdrop.color;
                tempColor.a = 0f;
                breathingUIBackdrop.color = tempColor;
            }

            //Inhale
            inhale = true;
            debugText.text = "Inhale";
            //iTween.ScaleTo(debugText.gameObject, Vector3.one * 1.5f, inhaleTimer);
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "easetype", iTween.EaseType.easeInOutSine, "time", inhaleTimer));
            //debugText.transform.DOScale(1.5f, inhaleTimer);
            breathingAudioSource.clip = inhaleAudio;
            breathingAudioSource.Play();

            //Tween BreathingUI
            MoveUIRef("x", 2.35f);
            iTween.FadeTo(breathingUIBackdrop.gameObject, 1f, inhaleTimer);

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            inhale = false;
            pause = true;
            debugText.text = "Hold";

            MoveUIRef("y", -2.05f);

            yield return new WaitForSeconds(pauseTimer);

            breathingAudioSource.Stop();

            //Exhale
            pause = false;
            exhale = true;
            debugText.text = "Exhale";
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutSine, "time", exhaleTimer));
            //debugText.transform.DOScale(1f, exhaleTimer);
            breathingAudioSource.clip = exhaleAudio;
            breathingAudioSource.Play();

            //Tween BreathingUI
            MoveUIRef("x", 0f);
            iTween.FadeTo(breathingUIBackdrop.gameObject, 0f, exhaleTimer);

            yield return new WaitForSeconds(exhaleTimer);

            //timeElapsed = 0f;

            //Pause
            inhale = false;
            pause = true;
            exhale = false;
            debugText.text = "Hold";

            MoveUIRef("y", 0);

            yield return new WaitForSeconds(pauseTimer);
        }
        while (breathingTimer < targetDuration);

        debugText.text = "";
        breathingInProgress = false;
        if (inTutorial)
        {
            inTutorial = false;
        }

        UpdateBreathingUIState(0f);
        postBreathingParticles.Play();

        yield return new WaitForSeconds(delayAfterCompletingExercise);

        canvas.SetActive(false);
        uiRef.transform.localPosition = Vector3.zero;
    }
    void MoveUIRef(string axis, float pos)
    {
        iTween.MoveTo(uiRef, iTween.Hash(axis, pos, "islocal", true, "easetype", iTween.EaseType.easeInOutSine, "time", pauseTimer));
    }
    /// <summary>
    /// This function is used to fade the breathing UI in or out.
    /// 1 = Fade in
    /// 0 = Fade out
    /// </summary>
    /// <param name="alpha"></param>
    public void UpdateBreathingUIState(float alpha)
    {
        StartCoroutine(UpdateBreathingUIStateCoroutine(alpha));

        /*foreach(Image image in breathingUIArray)
        {
            //image.DOFade(alpha, uiFadeDuration);
            iTween.FadeTo(image.gameObject, alpha, uiFadeDuration);
        }*/
    }
    IEnumerator UpdateBreathingUIStateCoroutine(float alpha)
    {
        if (alpha == 1)
        {
            //Fade Nimi out/UI in
            iTween.FadeTo(nimi, 0f, nimiFadeDuration);
            CurlNoiseParticleSystem.Emitter.ShapeEmitter.instance.Emit(nimiFadeDuration - 0.1f);

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            canvas.SetActive(true);
            uiAnimator.Play("BreathingUI_FadeIn");
        }
        else
        {
            //Fade UI out/Nimi in
            uiAnimator.Play("BreathingUI_FadeOut");

            yield return new WaitForSeconds(delayAfterCompletingExercise);

            iTween.FadeTo(nimi, 1f, nimiFadeDuration);

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            onBreathingFinishedEvent?.Invoke();
        }
    }

    bool breathingTimersUpdated = false;
    [SerializeField] Vector3 desiredInhalePos = new Vector3(2.35f, 0, 0);
    [SerializeField] Vector3 desiredExhalePos = new Vector3(0, -2.05f, 0);
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

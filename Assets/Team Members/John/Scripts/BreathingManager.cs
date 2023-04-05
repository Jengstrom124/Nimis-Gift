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

    [Header("Light Config: ")]
    public float desiredTopLightFadeValue;
    public float desiredBottomLightFadeValue;

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
    [SerializeField] bool inTutorial = false;
    Light topLight, bottomLight;
    float lightFadeDuration, topLightSV, bottomLightSV;

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

        topLight = NimiExperienceManager.instance.topLight;
        bottomLight = NimiExperienceManager.instance.bottomLight;
        topLightSV = NimiExperienceManager.instance.topLightStartValue;
        bottomLightSV = NimiExperienceManager.instance.bottomLightStartValue;
        lightFadeDuration = nimiFadeDuration + 2.5f;
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

            /*if (breathingUIBackdrop.color.a == 1)
            {
                var tempColor = breathingUIBackdrop.color;
                tempColor.a = 0f;
                breathingUIBackdrop.color = tempColor;
            }*/

            //Inhale
            elapsedTime = 0f;
            inhale = true;
            debugText.text = "Inhale";

            //Audio
            breathingAudioSource.clip = inhaleAudio;
            breathingAudioSource.Play();

            //Tween BreathingUI
            MoveUIRef("x", 2.35f);
            breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 1), inhaleTimer, true, true);
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "easetype", iTween.EaseType.easeInOutSine, "time", inhaleTimer));

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            elapsedTime = 0f;
            inhale = false;
            pause = true;
            debugText.text = "Hold";

            MoveUIRef("y", -2.05f);

            yield return new WaitForSeconds(pauseTimer);

            breathingAudioSource.Stop();

            //Exhale
            elapsedTime = 0f;
            pause = false;
            exhale = true;
            debugText.text = "Exhale";

            //Audio
            breathingAudioSource.clip = exhaleAudio;
            breathingAudioSource.Play();

            //Tween BreathingUI
            MoveUIRef("x", 0f);
            breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 0), exhaleTimer, true, true);
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutSine, "time", exhaleTimer));

            yield return new WaitForSeconds(exhaleTimer);

            //Pause
            elapsedTime = 0f;
            pause = true;
            exhale = false;
            debugText.text = "Hold";

            MoveUIRef("y", 0);

            yield return new WaitForSeconds(pauseTimer);

            inhale = true;

            yield return new WaitForSeconds(0.015f);
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

            elapsedTime = 0f;
            fadeLightsOut = true;

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            canvas.SetActive(true);
            uiAnimator.Play("BreathingUI_FadeIn");
        }
        else
        {
            //Fade UI out/Nimi in
            uiAnimator.Play("BreathingUI_FadeOut");

            elapsedTime = 0f;
            fadeLightsIn = true;

            yield return new WaitForSeconds(delayAfterCompletingExercise);

            iTween.FadeTo(nimi, 1f, nimiFadeDuration);

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            onBreathingFinishedEvent?.Invoke();
        }
    }

    bool breathingTimersUpdated = false;
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLightsOut, fadeLightsIn;
    private void Update()
    {
        if (breathingInProgress)
        {
            breathingTimer += Time.deltaTime;

            if (!inTutorial)
            {
                if (!breathingTimersUpdated)
                {
                    if (breathingTimer > targetDuration / 2)
                    {
                        if (inhale)
                        {
                            inhaleTimer = secondProgressionIncrease;
                            exhaleTimer = secondProgressionIncrease;
                            pauseTimer = secondProgressionIncrease;
                            breathingTimersUpdated = true;
                        }
                    }
                }
            }

            if(inhale)
            {
                //Fade Lights In
                if (elapsedTime < inhaleTimer)
                {
                    topLight.intensity = Mathf.Lerp(desiredTopLightFadeValue, topLightSV, elapsedTime / inhaleTimer);
                    bottomLight.intensity = Mathf.Lerp(desiredBottomLightFadeValue, bottomLightSV, elapsedTime / inhaleTimer);
                }
                else
                {
                    topLight.intensity = topLightSV;
                    bottomLight.intensity = bottomLightSV;
                }
                elapsedTime += Time.deltaTime;
            }

            if (exhale)
            {
                //Fade Lights Out
                if (elapsedTime < exhaleTimer)
                {
                    topLight.intensity = Mathf.Lerp(topLightSV, desiredTopLightFadeValue, elapsedTime / exhaleTimer);
                    bottomLight.intensity = Mathf.Lerp(bottomLightSV, desiredBottomLightFadeValue, elapsedTime / exhaleTimer);
                }
                else
                {
                    topLight.intensity = desiredTopLightFadeValue;
                    bottomLight.intensity = desiredBottomLightFadeValue;
                }
                elapsedTime += Time.deltaTime;
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

        if (fadeLightsOut)
        {
            if (elapsedTime < lightFadeDuration)
            {
                topLight.intensity = Mathf.Lerp(topLightSV, desiredTopLightFadeValue, elapsedTime / lightFadeDuration);
                bottomLight.intensity = Mathf.Lerp(bottomLightSV, desiredBottomLightFadeValue, elapsedTime / lightFadeDuration);
            }
            else
            {
                topLight.intensity = desiredTopLightFadeValue;
                bottomLight.intensity = desiredBottomLightFadeValue;
                fadeLightsOut = false;
            }
            elapsedTime += Time.deltaTime;
        }

        if (fadeLightsIn)
        {            
            if(elapsedTime < lightFadeDuration)
            {
                topLight.intensity = Mathf.Lerp(desiredTopLightFadeValue, topLightSV, elapsedTime / lightFadeDuration);
                bottomLight.intensity = Mathf.Lerp(desiredBottomLightFadeValue, bottomLightSV, elapsedTime / lightFadeDuration);
            }
            else
            {
                topLight.intensity = topLightSV;
                bottomLight.intensity = bottomLightSV;
                fadeLightsIn = false;
            }
            elapsedTime += Time.deltaTime;
        }
    }
}

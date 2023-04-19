using System.Collections;
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
    public float secondProgressionIncrease = 3f;
    public float targetDuration = 30f;
    public float firstPhaseDuration, secondPhaseDuration;
    public float delayBeforeExercise = 4.5f;
    public float delayAfterCompletingExercise = 2f;

    [Header("Light Config: ")]
    public float topLightFadeOutValue = 2f;
    public float bottomLightFadeOutValue = 1f;

    [Header("UI: ")]
    public Animator uiAnimator;
    public float uiFadeDuration = 2f;
    public float nimiFadeDuration = 2f;
    public GameObject uiRef;
    public Image[] breathingUIArray;
    public Image breathingUIBackdrop;
    public GameObject canvas;

    [Header("Audio: ")]
    public AudioSource breathingAudioSource;
    public AudioClip inhaleAudio, exhaleAudio;

    [Header("Debug/Refernces: ")]
    [SerializeField] bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;
    [SerializeField] bool inTutorial = false;
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLightsOut, fadeLightsIn, pauseEnvironmentParticles;
    bool breathingSettingsUpdated = false;
    Light topLight, bottomLight;
    float lightFadeDuration, topLightSV, bottomLightSV;
    bool tutorialComplete = false;

    [Header("Hacks: ")]
    public TMP_Text debugText;
    public GameObject nimi, nimiParticles, ambientParticlesGO;
    public AudioSource nimiAmbienceAudio;
    float nimiAmbienceStartVolume;
    public ParticleSystem postBreathingParticles, ambientParticles1, ambientParticles2;
    public float particleFadeValue = 0.1f;
    public Animator ambientParticles2Animator;

    //Events
    public event Action onBreathingFinishedEvent;
    public event Action onBreathingStartedEvent;
    bool introHack = true;

    private void Awake()
    {
        instance = this;

        FadeNimiOut();
    }

    private void Start()
    {
        debugText.text = "";

        topLight = NimiExperienceManager.instance.topLight;
        bottomLight = NimiExperienceManager.instance.bottomLight;
        topLightSV = NimiExperienceManager.instance.topLightStartValue;
        bottomLightSV = NimiExperienceManager.instance.bottomLightStartValue;
        lightFadeDuration = nimiFadeDuration + 2.5f;
        nimiAmbienceStartVolume = nimiAmbienceAudio.volume;
    }

    public void BeginBreathingExerciseTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExerciseTutorial;
        inTutorial = true;
        targetDuration = firstPhaseDuration;

        StartCoroutine(BreathingExcerciseCoroutine(0));
    }
    public void BeginBreathingExercise(float delayBeforeStarting)
    {
        //Update Breathing Settings
        if(!breathingSettingsUpdated)
        {
            inhaleTimer = secondProgressionIncrease;
            exhaleTimer = secondProgressionIncrease;
            targetDuration = secondPhaseDuration;
            breathingSettingsUpdated = true;
        }

        StartCoroutine(BreathingExcerciseCoroutine(delayBeforeStarting));
    }
    IEnumerator BreathingExcerciseCoroutine(float delay)
    {
        //Delay check for initialising breathing during a sequence
        if(delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        onBreathingStartedEvent?.Invoke();

        //Pre Tutorial has it's own sequence so this fading sequence only applies after tutorial
        if (!inTutorial)
        {
            UpdateBreathingUIState(1);

            //short delay to let all UI fade in before jumping straight into the exercise
            yield return new WaitForSeconds(nimiFadeDuration + delayBeforeExercise);
        }

        breathingInProgress = true;
        breathingTimer = 0f;

        //Debug Text
        debugText.text = "";

        do
        {
            #region Inhale
            breathingAudioSource.Stop();

            //Inhale
            elapsedTime = 0f;
            inhale = true;
            debugText.text = "Inhale";

            //Audio
            breathingAudioSource.clip = inhaleAudio;
            breathingAudioSource.Play();

            //Tween BreathingUI
            MoveUIRef("x", 2.35f, inhaleTimer);
            breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 1), inhaleTimer, true, true);
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "easetype", iTween.EaseType.easeInOutSine, "time", inhaleTimer));
            iTween.FadeTo(ambientParticlesGO, 1f, inhaleTimer);
            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeIn");
            if (pauseEnvironmentParticles)
            {
                PauseEnvironmentParticles(false);
            }

            #endregion

            yield return new WaitForSeconds(inhaleTimer);

            #region Pause/Hold
            //Pause
            elapsedTime = 0f;
            inhale = false;
            pause = true;
            debugText.text = "Hold";

            //Tweens
            MoveUIRef("y", -2.05f, pauseTimer);
            if(pauseEnvironmentParticles)
            {
                PauseEnvironmentParticles(true);
            }

            #endregion

            yield return new WaitForSeconds(pauseTimer);

            #region Exhale
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
            MoveUIRef("x", 0f, exhaleTimer);
            breathingUIBackdrop.CrossFadeColor(new Color(1, 1, 1, 0), exhaleTimer, true, true);
            iTween.ScaleTo(debugText.gameObject, iTween.Hash("scale", Vector3.one, "easetype", iTween.EaseType.easeOutSine, "time", exhaleTimer));
            iTween.FadeTo(ambientParticlesGO, particleFadeValue, exhaleTimer);
            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeOut");
            if (pauseEnvironmentParticles)
            {
                PauseEnvironmentParticles(false);
            }

            #endregion

            yield return new WaitForSeconds(exhaleTimer);

            #region Pause/Hold
            //Pause
            elapsedTime = 0f;
            pause = true;
            exhale = false;
            debugText.text = "Hold";

            MoveUIRef("y", 0, pauseTimer);
            if (pauseEnvironmentParticles)
            {
                PauseEnvironmentParticles(true);
            }
            #endregion

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
            tutorialComplete = true;
        }

        UpdateBreathingUIState(0f);
        postBreathingParticles.Play();

        yield return new WaitForSeconds(delayAfterCompletingExercise);

        canvas.SetActive(false);
        uiRef.transform.localPosition = Vector3.zero;
    }
    void MoveUIRef(string axis, float pos, float timer)
    {
        iTween.MoveTo(uiRef, iTween.Hash(axis, pos, "islocal", true, "easetype", iTween.EaseType.easeInOutSine, "time", timer));
    }
    void PauseEnvironmentParticles(bool pause)
    {
        if (pause)
        {
            ambientParticles1.Pause();

            if (ambientParticles2.isPlaying)
            {
                ambientParticles2.Pause();
            }

        }
        else
        {
            ambientParticles1.Play();

            if (ambientParticles2.isPaused)
            {
                ambientParticles2.Play();
            }
        }
    }

    #region Fading Breathing UI
    /// <summary>
    /// This function is used to fade the breathing UI in or out.
    /// 1 = Fade in
    /// 0 = Fade out
    /// </summary>
    /// <param name="alpha"></param>
    public void UpdateBreathingUIState(float alpha)
    {
        StartCoroutine(UpdateBreathingUIStateCoroutine(alpha));
    }
    IEnumerator UpdateBreathingUIStateCoroutine(float alpha)
    {
        if (alpha == 1)
        {
            //Fade Nimi out/UI in
            FadeNimiOut();
            /*iTween.FadeTo(nimi, iTween.Hash("alpha", 0f, "includechildren", false, "time", nimiFadeDuration));
            iTween.FadeTo(nimiParticles, 0f, nimiFadeDuration);*/

            elapsedTime = 0f;
            fadeLightsOut = true;

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            canvas.SetActive(true);
            uiAnimator.Play("BreathingUI_FadeIn");

            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeOut");

            yield return new WaitForSeconds(2f);

            iTween.FadeTo(ambientParticlesGO, particleFadeValue, 1.75f);
        }
        else
        {
            //Fade UI out
            uiAnimator.Play("BreathingUI_FadeOut");

            //Fade Environment Back In
            elapsedTime = 0f;
            fadeLightsIn = true;
            iTween.FadeTo(ambientParticlesGO, 1f, 5f);
            PauseEnvironmentParticles(false);
            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeIn");

            yield return new WaitForSeconds(delayAfterCompletingExercise);

            FadeNimiIn();

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            onBreathingFinishedEvent?.Invoke();
        }
    }
    public void FadeNimiOut()
    {
        if (introHack)
        {
            iTween.FadeTo(nimi, 0f, 0f);
            iTween.AudioTo(gameObject, iTween.Hash("audiosource", nimiAmbienceAudio, "volume", 0f, "easetype", iTween.EaseType.easeInOutSine, "time", 0f));
        }
        else
        {
            iTween.FadeTo(nimi, 0f, nimiFadeDuration);
            iTween.AudioTo(gameObject, iTween.Hash("audiosource", nimiAmbienceAudio, "volume", 0f, "easetype", iTween.EaseType.easeInOutSine, "time", nimiFadeDuration));
        }
    }
    public void FadeNimiIn()
    {
        //Hack for Intro Fade In
        if (introHack)
        {
            float introDuration = nimiFadeDuration + 3f;
            iTween.FadeTo(nimi, iTween.Hash("alpha", 0.35f, "includechildren", false, "time", introDuration));
            iTween.FadeTo(nimiParticles, 1f, introDuration);
            iTween.AudioTo(gameObject, iTween.Hash("audiosource", nimiAmbienceAudio, "volume", nimiAmbienceStartVolume, "easetype", iTween.EaseType.easeInOutSine, "time", introDuration));
            introHack = false;
        }
        else
        {
            iTween.FadeTo(nimi, iTween.Hash("alpha", 0.35f, "includechildren", false, "time", nimiFadeDuration));
            iTween.FadeTo(nimiParticles, 1f, nimiFadeDuration);
            iTween.AudioTo(gameObject, iTween.Hash("audiosource", nimiAmbienceAudio, "volume", nimiAmbienceStartVolume, "easetype", iTween.EaseType.easeInOutSine, "time", nimiFadeDuration));
        }
    }
    #endregion

    bool breathingTimersUpdated = false;
    private void Update()
    {
        if (breathingInProgress)
        {
            breathingTimer += Time.deltaTime;

            /*if (!inTutorial)
            {
                if (!breathingTimersUpdated)
                {
                    //Increase breathing progression midway through exercise (starting on next inhale)
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
            }*/

            if(inhale)
            {
                //Fade Lights In
                if (elapsedTime < inhaleTimer)
                {
                    topLight.intensity = Mathf.Lerp(topLightFadeOutValue, topLightSV, elapsedTime / inhaleTimer);
                    bottomLight.intensity = Mathf.Lerp(bottomLightFadeOutValue, bottomLightSV, elapsedTime / inhaleTimer);
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
                    topLight.intensity = Mathf.Lerp(topLightSV, topLightFadeOutValue, elapsedTime / exhaleTimer);
                    bottomLight.intensity = Mathf.Lerp(bottomLightSV, bottomLightFadeOutValue, elapsedTime / exhaleTimer);
                }
                else
                {
                    topLight.intensity = topLightFadeOutValue;
                    bottomLight.intensity = bottomLightFadeOutValue;
                }
                elapsedTime += Time.deltaTime;
            }
        }

        #region Fading Lights Before & After Breathing
        if (fadeLightsOut)
        {
            if (elapsedTime < lightFadeDuration)
            {
                topLight.intensity = Mathf.Lerp(topLightSV, topLightFadeOutValue, elapsedTime / lightFadeDuration);
                bottomLight.intensity = Mathf.Lerp(bottomLightSV, bottomLightFadeOutValue, elapsedTime / lightFadeDuration);
            }
            else
            {
                topLight.intensity = topLightFadeOutValue;
                bottomLight.intensity = bottomLightFadeOutValue;
                fadeLightsOut = false;
            }
            elapsedTime += Time.deltaTime;
        }

        if (fadeLightsIn)
        {            
            if(elapsedTime < lightFadeDuration)
            {
                topLight.intensity = Mathf.Lerp(topLightFadeOutValue, topLightSV, elapsedTime / lightFadeDuration);
                bottomLight.intensity = Mathf.Lerp(bottomLightFadeOutValue, bottomLightSV, elapsedTime / lightFadeDuration);
            }
            else
            {
                topLight.intensity = topLightSV;
                bottomLight.intensity = bottomLightSV;
                fadeLightsIn = false;
            }
            elapsedTime += Time.deltaTime;
        }
        #endregion
    }
}

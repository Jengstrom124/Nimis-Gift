using System.Collections;
using UnityEngine;
using System;

public class NimiBreathingManager : MonoBehaviour
{
    public static NimiBreathingManager instance;

    [Header("Breathing Settings")]
    public float delayBeforeExercise = 5f;
    public float delayAfterCompletingExercise = 5f;

    [Header("Stage 1 Breathing Timers")]
    [Tooltip("How long the breathing exercise will last")]
    [SerializeField] float stage1Duration = 60f;
    [Tooltip("Inhale/Exhale Durations")]
    [SerializeField] float stage1StartTimer, stage1FirstProgression, stage1SecondProgression;
    [Tooltip("How long into the duration should breathing timers update (20 = Update Timers AFTER 20 seconds etc)")]
    [SerializeField] float stage1FirstProgressionThreshold, stage1SecondProgressionThreshold;


    [Header("Stage 2 Breathing Timers")]
    [Tooltip("How long the breathing exercise will last")]
    [SerializeField] float stage2Duration = 90f;
    [Tooltip("Inhale/Exhale Durations")]
    [SerializeField] float stage2StartTimer, stage2FirstProgression, stage2SecondProgression;
    [Tooltip("How long into the duration should breathing timers update (20 = Update Timers AFTER 20 seconds etc)")]
    [SerializeField] float stage2FirstProgressionThreshold, stage2SecondProgressionThreshold;

    [Header("Debug/Refernces: ")]
    [SerializeField] float targetDuration = 30f;
    public float inhaleTimer, pauseTimer, exhaleTimer;
    public bool breathingInProgress = false;
    [SerializeField] float breathingTimer;
    [SerializeField] bool inhale, pause, exhale = false;
    public bool inTutorial = false;
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLightsOut, fadeLightsIn;
    public bool pauseEnvironmentParticles = true;
    bool stage1FirstProgressionComplete, stage1SecondProgressionComplete, stage2FirstProgressionComplete, stage2SecondProgressionComplete;
    bool stage1TimersComplete, stage2TimersComplete;
    float lightFadeDuration;
    public bool tutorialComplete = false;

    [Header("Hacks: ")]
    public Animator uiAnimator;
    public float nimiFadeDuration = 2.5f;
    public GameObject canvas;
    public AudioSource environmentTransitionAudioSource;
    public GameObject nimi, nimiParticles, ambientParticlesGO;
    public AudioSource nimiAmbienceAudio;
    float nimiAmbienceStartVolume;
    public ParticleSystem postBreathingParticles, environmentTransitionParticles;
    public float particleFadeValue = 0.1f;
    public Animator ambientParticles2Animator;

    //Events
    public event Action onBreathingFinishedEvent, onBreathingStartedEvent;
    public event Action<bool, float> onFadeBreathingLightsInEvent;
    public event Action<bool> onPauseEnvironmentParticlesEvent;
    public event Action onInhaleEvent, onExhaleEvent, onHoldAfterInhaleEvent, onHoldAfterExhaleEvent;
    public event Action<bool> onUnpauseParticlesHackEvent;
    public event Action clearTextHackEvent;
    bool introHack = true;

    private void Awake()
    {
        instance = this;

        FadeNimiOut();
    }

    private void Start()
    {
        lightFadeDuration = nimiFadeDuration + 2.5f;
        nimiAmbienceStartVolume = nimiAmbienceAudio.volume;

        NimiExperienceManager.instance.onEnvironmentUpgradeCompleteEvent += FadeNimiIn;
    }

    public void BeginBreathingExerciseTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExerciseTutorial;

        //Init Tutorial Timers
        inTutorial = true;
        inhaleTimer = stage1StartTimer;
        exhaleTimer = stage1StartTimer;
        targetDuration = stage1Duration;

        StartCoroutine(BreathingExcerciseCoroutine(0));
    }
    public void BeginBreathingExercise(float delayBeforeStarting)
    {
        //Update Breathing Timers
        inhaleTimer = stage2StartTimer;
        exhaleTimer = stage2StartTimer;
        targetDuration = stage2Duration;

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

        do
        {
            //Inhale
            elapsedTime = 0f;
            inhale = true;
            onInhaleEvent?.Invoke();

            //Lights
            onFadeBreathingLightsInEvent?.Invoke(true, inhaleTimer);

            yield return new WaitForSeconds(inhaleTimer);

            //Pause
            elapsedTime = 0f;
            inhale = false;
            pause = true;
            onHoldAfterInhaleEvent?.Invoke();

            yield return new WaitForSeconds(pauseTimer);

            //Exhale
            elapsedTime = 0f;
            pause = false;
            exhale = true;
            onExhaleEvent?.Invoke();

            //Lights
            onFadeBreathingLightsInEvent?.Invoke(false, exhaleTimer);

            yield return new WaitForSeconds(exhaleTimer);

            //Pause
            elapsedTime = 0f;
            pause = true;
            exhale = false;
            onHoldAfterExhaleEvent?.Invoke();
            
            yield return new WaitForSeconds(pauseTimer);

            inhale = true;

            yield return new WaitForSeconds(0.015f);
        }
        while (breathingTimer < targetDuration);

        PostBreathingCleanup();

        //Hack for giving a short delay before the transition audio plays
        yield return new WaitForSeconds(1.75f);

        environmentTransitionAudioSource.Play();
        iTween.AudioTo(environmentTransitionAudioSource.gameObject, iTween.Hash("audiosource", environmentTransitionAudioSource, "volume", 0.4f, "easetype", iTween.EaseType.easeInOutSine, "time", 2f));

        //Minusing the earlier time hack to prevent sequence disruption
        yield return new WaitForSeconds(delayAfterCompletingExercise - 1.75f);

        canvas.SetActive(false);
    }
    void PostBreathingCleanup()
    {
        clearTextHackEvent?.Invoke();
        breathingInProgress = false;
        if (inTutorial)
        {
            inTutorial = false;
            tutorialComplete = true;
        }

        UpdateBreathingUIState(0f);
        postBreathingParticles.Play();
        environmentTransitionParticles.Play();
    }

    #region Fading Breathing UI / HACKS

    //TODO: This should all be refactored

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

            //Fade Lights Out
            onFadeBreathingLightsInEvent?.Invoke(false, lightFadeDuration);

            yield return new WaitForSeconds(nimiFadeDuration + 1f);

            canvas.SetActive(true);
            uiAnimator.Play("BreathingUI_FadeIn_Final");

            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeOut");

            yield return new WaitForSeconds(2f);

            iTween.FadeTo(ambientParticlesGO, particleFadeValue, 1.75f);
        }
        else
        {
            //Fade UI out
            uiAnimator.Play("BreathingUI_FadeOut_Final");

            //Fade Environment Back In
            onFadeBreathingLightsInEvent?.Invoke(true, lightFadeDuration);

            iTween.FadeTo(ambientParticlesGO, 1f, 5f);
            onUnpauseParticlesHackEvent?.Invoke(false);
            if (tutorialComplete)
                ambientParticles2Animator.Play("AmbientParticleGlow_FadeIn");

            yield return new WaitForSeconds(delayAfterCompletingExercise);

            if (NimiExperienceManager.instance.nimiAuroraHack)
                NimiExperienceManager.instance.PlayAuroraAnimHack();

            //FadeNimiIn();

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

    private void Update()
    {
        if (breathingInProgress)
        {
            breathingTimer += Time.deltaTime;

            if(inTutorial)
            {
                #region Update Stage 1 Timers
                if (!stage1TimersComplete)
                {
                    if (!stage1FirstProgressionComplete)
                    {
                        //Only Update Timers on Inhale as that is after a full cycle
                        if(inhale)
                        {
                            //Update Timers After Specified Duration
                            if (breathingTimer > stage1FirstProgressionThreshold)
                            {
                                inhaleTimer = stage1FirstProgression;
                                exhaleTimer = stage1FirstProgression;
                                stage1FirstProgressionComplete = true;
                            }
                        }
                    }

                    if(!stage1SecondProgressionComplete)
                    {
                        if(inhale)
                        {
                            //Update Timers After Specified Duration
                            if (breathingTimer > stage1SecondProgressionThreshold)
                            {
                                inhaleTimer = stage1SecondProgression;
                                exhaleTimer = stage1SecondProgression;
                                stage1SecondProgressionComplete = true;
                                stage1TimersComplete = true;
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region Update Stage 2 Timers
                if (!stage2TimersComplete)
                {
                    if (!stage2FirstProgressionComplete)
                    {
                        if(inhale)
                        {
                            if (breathingTimer > stage2FirstProgressionThreshold)
                            {
                                inhaleTimer = stage2FirstProgression;
                                exhaleTimer = stage2FirstProgression;
                                stage2FirstProgressionComplete = true;
                            }
                        }
                    }

                    if (!stage2SecondProgressionComplete)
                    {
                        if(inhale)
                        {
                            if (breathingTimer > stage2SecondProgressionThreshold)
                            {
                                inhaleTimer = stage2SecondProgression;
                                exhaleTimer = stage2SecondProgression;
                                stage2SecondProgressionComplete = true;
                                stage2TimersComplete = true;
                            }
                        }
                    }
                }
                #endregion
            }

        }
    }
}

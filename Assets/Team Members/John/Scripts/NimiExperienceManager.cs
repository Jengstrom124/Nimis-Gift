using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Liminal.SDK.Core;
using Liminal.Core.Fader;
using TMPro;
using System;

public class NimiExperienceManager : MonoBehaviour
{
    public static NimiExperienceManager instance;

    public float introDialogueDelayTime = 3f;
    public float nimiFadeInDelayTime = 6.5f;
    public float environmentDialogueDelayTime = 2f;
    public float environmentFadeTime = 2f;
    public Material moonSkybox, voidSkyBox;
    [Tooltip("Delay given to the experience for the environment transitions/upgrades before continuing Nimi's Dialogue")]
    public float stage1EnvironmentUpgradeTimer, stage2EnvironmentUpgradeTimer;

    [Header("Nimi: ")]
    public Animator nimiAnimator;
    public AudioSource nimiAudioSource;
    public Animator nimiIntroAnimator;

    [Header("Dialogue Sequences")]
    [Tooltip("Delay before breathing Exercise starts after dialogue")]
    public float stage2ExerciseDelay = 3f;
    public float postStage2DialogueDelay = 3f;
    public float stage2MidDialogueDelay = 5f;
    public float stage3AuroraSequenceDelay = 5f;
    public float stage3MidDialogueDelay = 5f;
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue, stage2Dialogue, stage2DialogueCont, stage3Dialogue, stage3DialogueCont, endingDialogue;

    [Header("Environment Light Config: ")]
    public Light stage3ExtraRimLight;
    public Light topLight, bottomLight, rimLight;
    [HideInInspector]
    public Color topLightStartColour, bottomLightStartColour, rimLightStartColour; //environmentLightStartValue;
    public Color stage1TopLightColour, stage1BottomLightColour, stage1RimLightColour;
    public Color stage2TopLightColour, stage2BottomLightColour, stage2RimLightColour;
    public Color stage3TopLightColour, stage3BottomLightColour, stage3RimLightColour, stage3ExtraRimColour;
    public float stage3LightTransitionTimer = 10f;


    [Header("Environment Additions")]
    public ParticleSystem fireflies;
    public ParticleSystem moonRays, fallingLeaves, glowAmbientParticles;
    public AudioSource cricketAmbience, owlAmbiene, windAmbience, auroraAudioSource, windFlutesAmbience;
    public GameObject aurora, constellations;
    //public Terrain treeTerrain;

    [Header("Hacks")]
    public GameObject mindTreeEnvironment;
    [HideInInspector]
    public Transform dialogueCanvas;

    [Header("Debugs: ")]
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLights = false;
    public TMP_Text timerText;
    public bool useTimer = false;
    AmbientMode gradientAmbientMode;
    [SerializeField] bool triggerNimiExitAnim = false;
    [SerializeField] bool triggerNimiAuroraAnim = false;

    //Events
    public event Action onEnvironmentUpgradeCompleteEvent;

    private void Awake()
    {
        instance = this;

        topLightStartColour = topLight.color;
        bottomLightStartColour = bottomLight.color;
        rimLightStartColour = rimLight.color;

        topLight.color = Color.black;
        bottomLight.color = Color.black;
        rimLight.color = Color.black;

        /*if (topLight.intensity != 0)
        {
            topLight.intensity = 0f;
            bottomLight.intensity = 0f;
            //environmentLight.intensity = 0f;
            rimLight.gameObject.SetActive(false);
        }*/

        /*startingAmbientLightColour = RenderSettings.ambientLight;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientEquatorColor = Color.black;
        RenderSettings.ambientGroundColor = Color.black;*/

        /*gradientAmbientMode = RenderSettings.ambientMode;
        RenderSettings.ambientMode = AmbientMode.Skybox;*/
    }

    IEnumerator Start()
    {
        DialogueManager.instance.onDialogueFinishEvent += RevealMindTree;

        yield return new WaitForSeconds(5f);

        nimiIntroAnimator.SetTrigger("Begin");

        yield return new WaitForSeconds(nimiFadeInDelayTime);

        BreathingManager.instance.FadeNimiIn();

        yield return new WaitForSeconds(BreathingManager.instance.nimiFadeDuration + 1f);

        nimiAnimator.SetTrigger("OnSpawn");

        yield return new WaitForSeconds(introDialogueDelayTime);

        introDialogue.Interact(0);
    }

    void RevealMindTree()
    {
        DialogueManager.instance.onDialogueFinishEvent -= RevealMindTree;

        StartCoroutine(MindTreeSequenceCoroutine());
    }

    IEnumerator MindTreeSequenceCoroutine()
    {
        //yield return new WaitForSeconds(1.5f);

        //Fade Environment In
        //fadeLights = true;
        windFlutesAmbience.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windFlutesAmbience, "volume", 1f, "easetype", iTween.EaseType.easeInOutSine, "time", 3f));

        iTween.ColorTo(topLight.gameObject, stage1TopLightColour, environmentFadeTime);
        iTween.ColorTo(bottomLight.gameObject, stage1BottomLightColour, environmentFadeTime);
        iTween.ColorTo(rimLight.gameObject, stage1RimLightColour, environmentFadeTime);

        yield return new WaitForSeconds(1.5f);

        //Update Nimi
        nimiAnimator.SetTrigger("OnTreeSpawn");

        yield return new WaitForSeconds(environmentDialogueDelayTime);

        //Begin Next Dialogue Sequence
        mindTreeDialogue.Interact(environmentDialogueDelayTime);
        DialogueManager.instance.onDialogueFinishEvent += InitStage1Breathing;
    }

    void InitStage1Breathing()
    {
        DialogueManager.instance.onDialogueFinishEvent -= InitStage1Breathing;

        //Fade in Breathing UI/Fade Out Nimi
        BreathingManager.instance.UpdateBreathingUIState(1f);
        breathingTutorialDialogue.Interact(BreathingManager.instance.nimiFadeDuration + 4f);

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += PostStage1Breathing;
    }

    public void PostStage1Breathing()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= PostStage1Breathing;

        //First Environment Upgrade
        Stage1EnvironmentUpgrade();
        StartCoroutine(TriggerEnvironmentUpgradeEventCoroutine(stage1EnvironmentUpgradeTimer));

        //Start Dialogue Sequence
        stage2Dialogue.Interact(postStage2DialogueDelay + stage1EnvironmentUpgradeTimer);

        //Init First Phase of Breathing After Dialogue
        DialogueManager.instance.onDialogueFinishEvent += PostStage1BreathingContinued;
    }
 
    void Stage1EnvironmentUpgrade()
    {
        fallingLeaves.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windAmbience, "volume", 0.5f, "easetype", iTween.EaseType.easeInOutSine, "time", 4f));
        fireflies.Play();
        moonRays.Play();
        iTween.AudioTo(cricketAmbience.gameObject, iTween.Hash("audiosource", cricketAmbience, "volume", 0.48f, "easetype", iTween.EaseType.easeInOutSine, "time", 12f));
        owlAmbiene.Play();
        RenderSettings.skybox = moonSkybox;
        glowAmbientParticles.Play();
        constellations.SetActive(true);
    }
    void PostStage1BreathingContinued()
    {
        DialogueManager.instance.onDialogueFinishEvent -= PostStage1BreathingContinued;

        //Continue Sequence
        stage2DialogueCont.Interact(stage2MidDialogueDelay);
        DialogueManager.instance.onDialogueFinishEvent += BeginStage2Breathing;
    }
    public bool nimiAuroraHack = false;
    void BeginStage2Breathing()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginStage2Breathing;

        nimiAuroraHack = true;
        BreathingManager.instance.BeginBreathingExercise(1.5f);
        BreathingManager.instance.onBreathingFinishedEvent += PostStage2Breathing;
    }
    void PostStage2Breathing()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= PostStage2Breathing;

        //nimiAnimator.SetTrigger("SummonAurora");
        stage3Dialogue.Interact(1f + stage2EnvironmentUpgradeTimer);
        StartCoroutine(Stage2AuroraSequenceCoroutine());

        StartCoroutine(TriggerEnvironmentUpgradeEventCoroutine(stage2EnvironmentUpgradeTimer));
    }
    public void PlayAuroraAnimHack()
    {
        nimiAnimator.Play("Aurora_Summon");
        nimiAuroraHack = false;
    }
    IEnumerator Stage2AuroraSequenceCoroutine()
    {
        yield return new WaitForSeconds(7.5f);

        //Begin Aurora Here
        aurora.SetActive(true);
        auroraAudioSource.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", auroraAudioSource, "volume", 0.45f, "easetype", iTween.EaseType.easeInOutSine, "time", 7f));

        //Tween Environment Lights
        stage3ExtraRimLight.gameObject.SetActive(true);
        iTween.ColorTo(topLight.gameObject, stage3TopLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(bottomLight.gameObject, stage3BottomLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(rimLight.gameObject, stage3RimLightColour, stage3LightTransitionTimer);
        iTween.ColorTo(stage3ExtraRimLight.gameObject, stage3ExtraRimColour, stage3LightTransitionTimer);

        yield return new WaitForSeconds(stage3AuroraSequenceDelay);

        //Dialogue
        stage3DialogueCont.Interact(0);
        DialogueManager.instance.onDialogueFinishEvent += EndingSequence;
    }
    void EndingSequence()
    {
        DialogueManager.instance.onDialogueFinishEvent -= EndingSequence;
        endingDialogue.Interact(stage3MidDialogueDelay);

        //End Experience after Dialogue
        DialogueManager.instance.onDialogueFinishEvent += EndExperience;
    }
    public void TriggerNimiExitAnimation()
    {
        nimiAnimator.SetTrigger("OnExperienceEnd");
    }
    void EndExperience()
    {
        DialogueManager.instance.onDialogueFinishEvent -= EndExperience;

        //End Experience Here
        StartCoroutine(FadeAndExit(8f));
    }
    // This coroutine fades the camera and audio simultaneously over the same length of time.
    IEnumerator FadeAndExit(float fadeTime)
    {
        yield return new WaitForSeconds(6.5f);

        var elapsedTime = 0f; //instantiate a float with a value of 0 for use as a timer.
        var startingVolume = AudioListener.volume; //this gets the current volume of the audio listener so that we can fade it to 0 over time.

        ScreenFader.Instance.FadeTo(Color.black, fadeTime); // Tell the system to fade the camera to black over X seconds where X is the value of fadeTime.

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime; // Count up
            AudioListener.volume = Mathf.Lerp(startingVolume, 0f, elapsedTime / fadeTime); // This uses linear interpolation to change the volume of AudioListener over time.
            yield return new WaitForEndOfFrame(); // Tell the coroutine to wait for a frame to avoid completing this loop in a single frame.
        }

        // when the while-loop has ended, the audiolistener volume should be 0 and the screen completely black. However, for safety's sake, we should manually set AudioListener volume to 0.
        AudioListener.volume = 0f;

        ExperienceApp.End(); // This tells the platform to exit the experience.
    }

    private void Update()
    {
        if (useTimer)
            timerText.text = "" + Time.time;
        if(triggerNimiExitAnim)
        {
            nimiAnimator.Play("Nimi_Exit"); 
            triggerNimiExitAnim = false;
        }
        if(triggerNimiAuroraAnim)
        {
            nimiAnimator.Play("Aurora_Summon");
            triggerNimiAuroraAnim = false;
        }

        /*if (fadeLights)
        {
            if (elapsedTime < environmentFadeTime)
            {
                topLight.intensity = Mathf.Lerp(0, topLightStartValue, elapsedTime / environmentFadeTime);
                bottomLight.intensity = Mathf.Lerp(0, bottomLightStartValue, elapsedTime / environmentFadeTime);
                //environmentLight.intensity = Mathf.Lerp(0, environmentLightStartValue, elapsedTime / environmentFadeTime);

            }
            else
            {
                topLight.intensity = topLightStartValue;
                bottomLight.intensity = bottomLightStartValue;
                //environmentLight.intensity = environmentLightStartValue;
                rimLight.gameObject.SetActive(true);
                fadeLights = false;
            }

            elapsedTime += Time.deltaTime;
        }*/
    }

    IEnumerator TriggerEnvironmentUpgradeEventCoroutine(float environmentUpgradeDelay)
    {
        yield return new WaitForSeconds(environmentUpgradeDelay);

        onEnvironmentUpgradeCompleteEvent?.Invoke();
    }

    public void UpgradeEnvironmentDebug()
    {
        Stage1EnvironmentUpgrade();
    }
}

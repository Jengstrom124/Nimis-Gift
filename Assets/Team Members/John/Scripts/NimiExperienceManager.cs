using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Liminal.SDK.Core;
using Liminal.Core.Fader;
using TMPro;

public class NimiExperienceManager : MonoBehaviour
{
    public static NimiExperienceManager instance;
    public float introDialogueDelayTime = 3f;
    public float nimiFadeInDelayTime = 6.5f;
    public float environmentDialogueDelayTime = 2f;
    public float environmentFadeTime = 2f;
    public Material moonSkybox, voidSkyBox, auroraSkybox, foliageMat;
    //public Shader foliageSwayShader, foliageDefaultShader;

    [Header("Nimi: ")]
    public Animator nimiAnimator;
    public AudioSource nimiAudioSource;
    public Animator nimiIntroAnimator;

    [Header("Dialogue Sequences")]
    public float postTutorialDialogueDelay = 2.5f;
    [Tooltip("Delay before breathing Exercise starts after dialogue")]
    public float stage2ExerciseDelay = 3f;
    public float postStage2DialogueDelay = 3f;
    public float stage2MidDialogueDelay = 5f;
    public float stage3AuroraSequenceDelay = 5f;
    public float stage3MidDialogueDelay = 5f;
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue, stage2Dialogue, stage2DialogueCont, stage3Dialogue, stage3DialogueCont, endingDialogue;

    [Header("Environment Additions")]
    public ParticleSystem fireflies;
    public ParticleSystem moonRays, fallingLeaves, glowAmbientParticles;
    public AudioSource cricketAmbience, owlAmbiene, windAmbience, auroraAudioSource, windFlutesAmbience;
    public GameObject aurora, constellations;
    //public Terrain treeTerrain;

    [Header("Hacks")]
    public GameObject mindTreeEnvironment;
    public GameObject environmentLights;
    public Light topLight, bottomLight, environmentLight;
    public float topLightStartValue, bottomLightStartValue, environmentLightStartValue;
    public Transform dialogueCanvas;

    [Header("Debugs: ")]
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLights = false;
    public TMP_Text timerText;
    public bool useTimer = false;
    AmbientMode gradientAmbientMode;

    private void Awake()
    {
        instance = this;

        topLightStartValue = topLight.intensity;
        bottomLightStartValue = bottomLight.intensity;
        environmentLightStartValue = environmentLight.intensity;

        if (environmentLight.intensity != 0)
        {
            topLight.intensity = 0f;
            bottomLight.intensity = 0f;
            environmentLight.intensity = 0f;
        }

        /*startingAmbientLightColour = RenderSettings.ambientLight;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientEquatorColor = Color.black;
        RenderSettings.ambientGroundColor = Color.black;*/

        gradientAmbientMode = RenderSettings.ambientMode;
        RenderSettings.ambientMode = AmbientMode.Skybox;
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
        fadeLights = true;
        windFlutesAmbience.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windFlutesAmbience, "volume", 1f, "easetype", iTween.EaseType.easeInOutSine, "time", 3f));

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
        breathingTutorialDialogue.Interact(BreathingManager.instance.nimiFadeDuration + 2.5f);

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += PostStage1Breathing;
    }

    public void PostStage1Breathing()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= PostStage1Breathing;

        //First Environment Upgrade
        Stage1EnvironmentUpgrade();

        //Start Dialogue Sequence
        stage2Dialogue.Interact(postStage2DialogueDelay);

        //Init First Phase of Breathing After Dialogue
        DialogueManager.instance.onDialogueFinishEvent += PostStage1BreathingContinued;
    }
 
    void Stage1EnvironmentUpgrade()
    {
        /*TerrainData terrainData = treeTerrain.terrainData;
        terrainData.wavingGrassSpeed = 0.25f;
        treeTerrain.terrainData = terrainData;*/
        //treeTerrain.terrainData.wavingGrassSpeed = 0.25f;
        fallingLeaves.Play();
        iTween.AudioTo(gameObject, iTween.Hash("audiosource", windAmbience, "volume", 0.5f, "easetype", iTween.EaseType.easeInOutSine, "time", 4f));
        fireflies.Play();
        moonRays.Play();
        iTween.AudioTo(cricketAmbience.gameObject, iTween.Hash("audiosource", cricketAmbience, "volume", 0.48f, "easetype", iTween.EaseType.easeInOutSine, "time", 12f));
        owlAmbiene.Play();
        RenderSettings.skybox = moonSkybox;
        glowAmbientParticles.Play();
        RenderSettings.ambientMode = gradientAmbientMode;
        //foliageMat.shader = foliageSwayShader;
        //constellations.SetActive(true);
        /*RenderSettings.ambientLight = startingAmbientLightColour;
        RenderSettings.ambientEquatorColor = startingAmbientEquatorColour;
        RenderSettings.ambientGroundColor = startingAmbientGroundColour;*/
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
        stage3Dialogue.Interact(1f);
        StartCoroutine(Stage2AuroraSequenceCoroutine());
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
        StartCoroutine(FadeAndExit(5f));
    }
    // This coroutine fades the camera and audio simultaneously over the same length of time.
    IEnumerator FadeAndExit(float fadeTime)
    {
        yield return new WaitForSeconds(5.5f);

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

        if (fadeLights)
        {
            if (elapsedTime < environmentFadeTime)
            {
                topLight.intensity = Mathf.Lerp(0, topLightStartValue, elapsedTime / environmentFadeTime);
                bottomLight.intensity = Mathf.Lerp(0, bottomLightStartValue, elapsedTime / environmentFadeTime);
                environmentLight.intensity = Mathf.Lerp(0, environmentLightStartValue, elapsedTime / environmentFadeTime);

            }
            else
            {
                topLight.intensity = topLightStartValue;
                bottomLight.intensity = bottomLightStartValue;
                environmentLight.intensity = environmentLightStartValue;
                fadeLights = false;
            }

            elapsedTime += Time.deltaTime;
        }
    }

    public void UpgradeEnvironmentDebug()
    {
        Stage1EnvironmentUpgrade();
    }
}

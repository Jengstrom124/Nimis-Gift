using System.Collections;
using UnityEngine;
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
    [Tooltip("Delay given to the experience for the environment transitions/upgrades before continuing Nimi's Dialogue")]
    public float stage1EnvironmentUpgradeTimer, stage2EnvironmentUpgradeTimer;

    [Header("Nimi Hack: ")]
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
    public float stage3LightTransitionTimer = 10f;

    [Header("Debugs: ")]
    [SerializeField] float elapsedTime = 0f;
    [SerializeField] bool fadeLights = false;
    public TMP_Text timerText;
    public bool useTimer = false;
    [SerializeField] bool triggerNimiExitAnim = false;
    [SerializeField] bool triggerNimiAuroraAnim = false;

    //Events
    public event Action onRevealMindTreeEvent, onEnvironmentUpgradeCompleteEvent, onStage1EnvironmentEvent, onStage2EnvironmentEvent;

    private void Awake()
    {
        instance = this;
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
        onRevealMindTreeEvent?.Invoke();

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
        onStage1EnvironmentEvent?.Invoke();
        StartCoroutine(TriggerEnvironmentUpgradeEventCoroutine(stage1EnvironmentUpgradeTimer));

        //Start Dialogue Sequence
        stage2Dialogue.Interact(postStage2DialogueDelay + stage1EnvironmentUpgradeTimer);

        //Init First Phase of Breathing After Dialogue
        DialogueManager.instance.onDialogueFinishEvent += PostStage1BreathingContinued;
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
        onStage2EnvironmentEvent?.Invoke();

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
        //Animation Debugs
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
    }

    //Hack for Fading Nimi In After The Environment Upgrade is complete
    IEnumerator TriggerEnvironmentUpgradeEventCoroutine(float environmentUpgradeDelay)
    {
        yield return new WaitForSeconds(environmentUpgradeDelay);

        onEnvironmentUpgradeCompleteEvent?.Invoke();
    }

    public void UpgradeEnvironmentDebug()
    {
        onStage1EnvironmentEvent?.Invoke();
    }
}

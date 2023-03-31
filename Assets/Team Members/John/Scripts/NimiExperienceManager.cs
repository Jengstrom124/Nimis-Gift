using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NimiExperienceManager : MonoBehaviour
{
    public static NimiExperienceManager instance;
    public float introDialogueDelayTime = 3f;
    public float environmentDialogueDelayTime = 2f;

    public Animator nimiAnimator;

    [Header("Dialogue Sequences")]
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue;

    [Header("Environment Additions")]
    public ParticleSystem fireflies;
    public ParticleSystem moonRays;
    public AudioSource nightAmbience;

    [Header("Hacks")]
    public GameObject mindTreeEnvironment;
    public GameObject[] environmentLights;
    public Transform dialogueCanvas;
    public bool canInteractWithTree = false;

    [Header("Debugs: ")]
    public bool testFirstEnvironmentAddition = false;

    public event Action onTreeRevealEvent;

    private void Awake()
    {
        instance = this;

        foreach (GameObject light in environmentLights)
        {
            light.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.instance.onDialogueFinishEvent += RevealMindTree;

        StartCoroutine(InitSequenceCoroutine());

        if(testFirstEnvironmentAddition)
        {
            Debug.Log("Testing Environment Additions");
            EnableFirstEnvironmentAddition();
        }
    }

    IEnumerator InitSequenceCoroutine()
    {
        yield return new WaitForSeconds(introDialogueDelayTime);

        introDialogue.Interact();
    }

    void RevealMindTree()
    {
        DialogueManager.instance.onDialogueFinishEvent -= RevealMindTree;

        //Update Nimi
        nimiAnimator.SetTrigger("OnTreeSpawn");

        StartCoroutine(MindTreeSequenceCoroutine());
    }

    IEnumerator MindTreeSequenceCoroutine()
    {

        yield return new WaitForSeconds(1f);

        //Fade Environment In
        //onTreeRevealEvent?.Invoke();
        //iTween.FadeTo(mindTreeEnvironment, 1f, 5f);
        foreach (GameObject light in environmentLights)
        {
            light.SetActive(true);
        }

        //Begin Next Dialogue Sequence
        dialogueCanvas.position = new Vector3(-4.45f, -3f, -2.68f);
        dialogueCanvas.rotation = Quaternion.Euler(0, -40f, 0);

        yield return new WaitForSeconds(environmentDialogueDelayTime);

        mindTreeDialogue.Interact();
        DialogueManager.instance.onDialogueFinishEvent += InitBreathingTutorial;
    }

    void InitBreathingTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= InitBreathingTutorial;

        BreathingManager.instance.UpdateBreathingUIState(1f);
        StartCoroutine(InitTutorialDialogueCoroutine());

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += EndTutorial;
    }
    IEnumerator InitTutorialDialogueCoroutine()
    {
        yield return new WaitForSeconds(BreathingManager.instance.nimiFadeDuration + 3f);

        breathingTutorialDialogue.Interact();
    }

    void EndTutorial()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= EndTutorial;

        postBreathingTutorialDialgoue.Interact();

        //Setup tree interaction
        DialogueManager.instance.onDialogueFinishEvent += EnableTreeInteraction;

        //Fake tree interaction for now
        //DialogueManager.instance.onDialogueFinishEvent += BeginBreathingExercisePlaceholder;
    }

    void EnableTreeInteraction()
    {
        DialogueManager.instance.onDialogueFinishEvent -= EnableTreeInteraction;

        canInteractWithTree = true;
        //UIHack(false);
        //BreathingManager.instance.onBreathingFinishedEvent += UpdateUIHack;

        BreathingManager.instance.onBreathingFinishedEvent += EnableFirstEnvironmentAddition;
    }

    void BeginBreathingExercisePlaceholder()
    {
        DialogueManager.instance.onDialogueFinishEvent -= BeginBreathingExercisePlaceholder;

        BreathingManager.instance.BeginBreathingExercise();
    }

    void EnableFirstEnvironmentAddition()
    {
        BreathingManager.instance.onBreathingFinishedEvent -= EnableFirstEnvironmentAddition;

        fireflies.Play();
        moonRays.Play();
        iTween.AudioTo(nightAmbience.gameObject, iTween.Hash("audiosource", nightAmbience,"volume", 0.4f, "easetype", iTween.EaseType.easeInOutSine ,"time", 10f));
    }

    //Hacking turning on & off the UI because the UI canvas are blocking the raycast/interaction from reaching the tree
    void UpdateUIHack()
    {
        UIHack(false);
    }
    public void UIHack(bool turnUIOn)
    {
        dialogueCanvas.gameObject.SetActive(turnUIOn);
        //breathingGO.SetActive(turnUIOn);
    }
}

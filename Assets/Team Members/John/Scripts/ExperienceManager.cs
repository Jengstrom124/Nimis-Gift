using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public float introDialogueDelayTime = 3f;
    public float environmentDialogueDelayTime = 2f;

    [Header("Dialogue Sequences")]
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue;

    [Header("Hacks")]
    public GameObject environmentHack;

    private void Awake()
    {
        environmentHack.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.instance.onDialogueFinishEvent += RevealMindTree;

        Invoke("InitSequence", introDialogueDelayTime);
    }

    void InitSequence()
    {
        introDialogue.Interact();
    }

    void RevealMindTree()
    {
        DialogueManager.instance.onDialogueFinishEvent -= RevealMindTree;

        //Fade Environment In
        environmentHack.SetActive(true);

        //Begin Next Dialogue Sequence
        Invoke("MindTreeDialogue", environmentDialogueDelayTime);
    }
    void MindTreeDialogue()
    {
        mindTreeDialogue.Interact();
        DialogueManager.instance.onDialogueFinishEvent += InitBreathingTutorial;
    }

    void InitBreathingTutorial()
    {
        DialogueManager.instance.onDialogueFinishEvent -= InitBreathingTutorial;

        breathingTutorialDialogue.Interact();

        DialogueManager.instance.onDialogueFinishEvent += BreathingManager.instance.BeginBreathingExerciseTutorial;
        BreathingManager.instance.onBreathingFinishedEvent += EndTutorial;
    }

    void EndTutorial()
    {
        BreathingManager.instance.onBreathingFinishedEvent += EndTutorial;

        postBreathingTutorialDialgoue.Interact();

        //Setup tree interaction
    }
}

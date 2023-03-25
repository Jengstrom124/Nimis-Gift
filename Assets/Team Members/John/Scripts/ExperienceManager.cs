using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public float introDialogueDelayTime = 3f;
    public float environmentDialogueDelayTime = 2f;

    public Animator nimiAnimator;

    [Header("Dialogue Sequences")]
    public DialogueTrigger introDialogue, mindTreeDialogue;
    public DialogueTrigger breathingTutorialDialogue, postBreathingTutorialDialgoue;

    [Header("Hacks")]
    public GameObject environmentHack;
    public Transform dialogueCanvas;

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

        //Update Nimi
        nimiAnimator.SetTrigger("OnTreeSpawn");

        StartCoroutine(MindTreeSequenceCoroutine());
    }

    IEnumerator MindTreeSequenceCoroutine()
    {

        yield return new WaitForSeconds(1f);
        //Fade Environment In
        environmentHack.SetActive(true);

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

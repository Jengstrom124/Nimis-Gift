using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public float introDialogueDelayTime = 3f;

    [Header("Dialogue Sequences")]
    public DialogueTrigger introDialogue, mindTreeDialogue;

    [Header("Hacks")]
    public GameObject environmentHack;

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

        environmentHack.SetActive(true);
        mindTreeDialogue.Interact();
    }
}

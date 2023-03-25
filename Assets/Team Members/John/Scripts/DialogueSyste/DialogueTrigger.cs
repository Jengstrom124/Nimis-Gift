using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntryHolder
{
    public List<DialogueEntry> dialogueEntries;
}
public class DialogueTrigger : MonoBehaviour
{
    //Using this setup for now whilst NPC's don't have anything unique about them other then their dialogue (so no need to code anything NPC specific)
    [Header("NPC SETUP")]
    public bool triggerDialogueFinisedEvent = false;
    public bool triggerDialogueStartedEvent = false;
    public bool multipleDialogueEntries = false;
    public bool repeatDialogueEntries = false;
    public bool stopTriggerAfterDialogue = false;
    bool dialogueLaunched;

    [Header("Cutscene Setup")]
    public bool triggerOnAwake = false;
    public bool triggerOnEnableUsingTimeline = false;
    public bool useTriggerBox = false;

    private void Start()
    {
        if (triggerOnAwake)
        {
            DialogueManager.instance.StartDialogue(diagloueEntries, triggerDialogueFinisedEvent, triggerDialogueStartedEvent);
        }
    }

    //This holds all the dialogue entries
    //TODO: Maybe have multiple lists for when player tries to interact again to have more variations in dialogue?
    public List<DialogueEntry> diagloueEntries;

    //If we want to create a library of dialogue entries we can use this
    public List<DialogueEntryHolder> myDialogueEntries;

    int index = 0;

    public void Interact()
    {
        if(!stopTriggerAfterDialogue)
        {
            StartDialogue();
        }
        else
        {
            if (dialogueLaunched)
                return;
            else
            {
                dialogueLaunched = true;

                StartDialogue();
            }
        }
        
    }
    void StartDialogue()
    {
        if (!multipleDialogueEntries)
            TriggerDialogue();
        else
            MultipleDialogueTrigger();
    }

    void MultipleDialogueTrigger()
    {
        //Send the dialogue of the current index then increment the index
        if(index < myDialogueEntries.Count)
        {
            DialogueManager.instance.StartDialogue(myDialogueEntries[index].dialogueEntries, triggerDialogueFinisedEvent, triggerDialogueStartedEvent);
            index++;
        }
        else
        {
            if (repeatDialogueEntries)
            {
                index = 0;
                DialogueManager.instance.StartDialogue(myDialogueEntries[index].dialogueEntries, triggerDialogueFinisedEvent, triggerDialogueStartedEvent);
                index++;
            }
            else
            {
                //send the last entry in the list
                DialogueManager.instance.StartDialogue(myDialogueEntries[index - 1].dialogueEntries, triggerDialogueFinisedEvent, triggerDialogueStartedEvent);
            }
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(diagloueEntries, triggerDialogueFinisedEvent, triggerDialogueStartedEvent);
    }
}

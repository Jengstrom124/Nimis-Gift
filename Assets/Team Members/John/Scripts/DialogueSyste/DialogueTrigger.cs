using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntryHolder
{
    public List<DialogueEntry> dialogueEntries;
}
public class DialogueTrigger : MonoBehaviour
{
    [Header("Settings: ")]
    public bool triggerDialogueFinisedEvent = false;
    public bool triggerDialogueStartedEvent = false;
    public bool multipleDialogueEntries = false;
    public bool repeatDialogueEntries = false;
    public bool stopTriggerAfterDialogue = false;

    //This holds all the dialogue entries
    public List<DialogueEntry> diagloueEntries;

    //If we want to create a library of dialogue entries we can use this
    public List<DialogueEntryHolder> myDialogueEntries;

    int index = 0;

    public void Interact(float delay)
    {
        StartCoroutine(InteractCoroutine(delay));
    }
    IEnumerator InteractCoroutine(float delay)
    {
        if(delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        StartDialogue();
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

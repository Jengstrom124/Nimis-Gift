using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager instance;

	[Header("General Setup")]
	public Animator textAnimator;
	public TMP_Text dialogueText;
	public AudioSource audioManager;
	public AudioSource interactAudioSource;
	public AudioSource customSFXAudioSource;
	public GameObject narrativeDialogueGO;
	public AudioClip interactSound;

	[Header("Dialogue Settings")]
	public float dialogueSpeed = 7.5f;
	public float dialogueFadeSpeed = 3f;
	public float defaultDialogueVolume = 0.4f;
	float defaultDialogueSpeed;

	[Space]
	public bool usePitchVariation = true;
	public Vector2 volumeRandomisationRange = new Vector2(0.3f, 0.4f);
	[Range(0.05f, 0.5f)]
	public float pitchChangeMultiplier = 0.1f;

	[Space]
	public float sfxDelay = 0.015f;
	public float specialCharacterDelay = 0.015f;
	public float highlightedWordDelay = 0.05f;
	public float interruptDelay = 0.015f;

	public bool useTriggerAudioOnly = false;
	public bool useDialogueSound = true;

	//These are update with dialogue triggers
	List<DialogueEntry> currentDialogueEntries;
	AudioClip nonPlayerDialogueAudio;

	[Header("Debugs/References: ")]
	bool dialogueInProgress = false;
	public bool dialogueActive;
	int index;

	[Header("Testing/Hacks: ")]
	public bool testUsingSFXDelay;
	public bool standAloneCutsceneControlls;
	bool triggerDialogueFinishedEvent;
	public event System.Action onDialogueFinishEvent;
	public event System.Action onDialogueStartedEvent;
	int dialogueIndexCheck;

	private void Awake()
    {
		instance = this;

		defaultDialogueSpeed = dialogueSpeed;

		audioManager.volume = defaultDialogueVolume;
    }

	public void StartDialogue(List<DialogueEntry> dialogueEntriesRecieved, AudioClip newNonPlayerDialogueAudio, bool triggerDialogueFinished, bool triggerDialogueStarted)
	{
		//When dialogue begins, update the game state
		//if(GameManager.instance != null)
			//GameManager.instance.UpdateDialogueGameState(true);

		index = 0;
		currentDialogueEntries = dialogueEntriesRecieved;
		//canTalk = true;
		nonPlayerDialogueAudio = newNonPlayerDialogueAudio;

		triggerDialogueFinishedEvent = triggerDialogueFinished;

		//Bit of a hack
		if (useTriggerAudioOnly)
			audioManager.clip = newNonPlayerDialogueAudio;

		dialogueActive = true;

		if (triggerDialogueStarted)
			onDialogueStartedEvent?.Invoke();

		dialogueIndexCheck = 0;

		StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
	}

    private void Update()
    {
		//Hack for TakoIntro as controller doesn't exist yet
		if (standAloneCutsceneControlls && dialogueActive)
		{
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
			{
				ContinueDialogue();
			}
		}
    }

	bool dialogueFinished = false;
    public void ContinueDialogue()
	{
		if(!dialogueFinished)
        {
			//otherwise start next dialogue or end dialogue if no more dialogue left
			if (index >= currentDialogueEntries.Count)
			{
				EndDialogue();
			}
			else
			{
				StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
			}
		}
	}

	bool dialogueSFXWasTempDisabled = false;
	IEnumerator DisplayNextDialogueCoroutine(string currentDialogue)
	{
		dialogueInProgress = true;
		dialogueSFXWasTempDisabled = false;

		yield return new WaitForSeconds(0.15f);

		dialogueText.text = currentDialogue;

		//Fade Text In
		dialogueText.DOFade(1, 3f);

		//Handle Dialogue SFX------------------------------
		if (useDialogueSound)
		{
			//Adjust Pitch/Volume
			if (usePitchVariation)
			{
				audioManager.volume = Random.Range(volumeRandomisationRange.x, volumeRandomisationRange.y);
				audioManager.pitch = Random.Range(1 - pitchChangeMultiplier, 1 + pitchChangeMultiplier);
			}

			if (testUsingSFXDelay)
			{
				yield return new WaitForSeconds(sfxDelay);
				audioManager.Play();
			}
			else
				audioManager.Play();
		}

		yield return new WaitForSeconds(dialogueSpeed);

		OnCurrentDialogueFinished();
	}

	void OnCurrentDialogueFinished()
	{
		dialogueInProgress = false;
		if (dialogueSFXWasTempDisabled)
			useDialogueSound = true;

		//Progress Dialogue Position
		index++;

		//Fade Dialogue Out
		dialogueText.DOFade(0, dialogueFadeSpeed);

		Invoke("ContinueDialogue", dialogueFadeSpeed);
	}

	void EndDialogue()
    {
		dialogueActive = false;
		dialogueFinished = false;
		dialogueText.text = "";
	}
}

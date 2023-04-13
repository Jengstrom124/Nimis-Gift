using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager instance;

	[Header("General Setup")]
	public Animator textAnimator;
	public GameObject dialogueCanvas;
	public TMP_Text dialogueText;
	public AudioSource dialogueAudioSource;
	public AudioClip[] dialogueAudioClips;
	public AudioSource interactAudioSource;
	public AudioClip interactSound;

	[Header("Dialogue Settings")]
	public float dialogueSpeed = 7.5f;
	public float dialogueFadeOutSpeed = 3f;
	public float dialogueFadeInSpeed = 3f;
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

		dialogueAudioSource.volume = defaultDialogueVolume;
	}

	public void StartDialogue(List<DialogueEntry> dialogueEntriesRecieved, bool triggerDialogueFinished, bool triggerDialogueStarted)
	{
		dialogueCanvas.SetActive(true);

		index = 0;
		currentDialogueEntries = dialogueEntriesRecieved;

		triggerDialogueFinishedEvent = triggerDialogueFinished;

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
		//Init Dialogue
		dialogueInProgress = true;
		dialogueSFXWasTempDisabled = false;

		//Init Dialogue Speed
		if (currentDialogueEntries[index].useCustomDialogueSpeed)
			dialogueSpeed = currentDialogueEntries[index].customDelayTime;
		else
			dialogueSpeed = defaultDialogueSpeed;

		dialogueAudioSource.clip = dialogueAudioClips[Random.Range(0, dialogueAudioClips.Length - 1)];

		if(currentDialogueEntries[index].endingDialogueHack)
        {
			NimiExperienceManager.instance.PlayNimiExitAnimation();
        }

		yield return new WaitForSeconds(0.15f);

		dialogueText.text = currentDialogue;

		//Fade Text In
		textAnimator.Play("Dialogue_FadeIn");
		//iTween.FadeTo(dialogueText.gameObject, 1f, dialogueFadeInSpeed);
		//dialogueText.DOFade(1, dialogueFadeInSpeed);

		//Handle Dialogue SFX------------------------------
		if (useDialogueSound)
		{
			//Adjust Pitch/Volume
			if (usePitchVariation)
			{
				dialogueAudioSource.volume = Random.Range(volumeRandomisationRange.x, volumeRandomisationRange.y);
				dialogueAudioSource.pitch = Random.Range(1 - pitchChangeMultiplier, 1 + pitchChangeMultiplier);
			}

			if (testUsingSFXDelay)
			{
				yield return new WaitForSeconds(sfxDelay);
				dialogueAudioSource.Play();
			}
			else
				dialogueAudioSource.Play();
		}

		yield return new WaitForSeconds(dialogueSpeed);

		StartCoroutine(OnCurrentDialogueFinished());
	}

	IEnumerator OnCurrentDialogueFinished()
	{
		dialogueInProgress = false;
		if (dialogueSFXWasTempDisabled)
			useDialogueSound = true;

		//Progress Dialogue Position
		index++;

		//Fade Dialogue Out
		textAnimator.Play("Dialogue_FadeOut");
		//dialogueText.DOFade(0, dialogueFadeOutSpeed);
		//iTween.FadeTo(dialogueText.gameObject, 0f, dialogueFadeOutSpeed);

		yield return new WaitForSeconds(dialogueFadeOutSpeed);

		ContinueDialogue();
	}

	public void EndDialogue()
    {
		dialogueActive = false;
		dialogueFinished = false;
		dialogueText.text = "";
		dialogueCanvas.SetActive(false);

		if (triggerDialogueFinishedEvent)
			onDialogueFinishEvent?.Invoke();
	}
}

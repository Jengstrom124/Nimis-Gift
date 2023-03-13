using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
	public static DialogueManager instance;

	[Header("General Setup")]
	public Animator animator;
	public AudioSource audioManager;
	public AudioSource interactAudioSource;
	public AudioSource customSFXAudioSource;
	public GameObject playerDialogueGO;
	public GameObject npcDialogueGO;
	public GameObject narrativeDialogueGO;
	public GameObject onScreenDisplayGO;
	public AudioClip interactSound;

	[Header("Dialogue Settings")]
	public float dialogueSpeed = 0.3f;
	public float defaultDialogueVolume = 0.4f;
	float defaultDialogueSpeed;

	[Space]
	public bool useCustomSFXFrequency = false;
	[Tooltip("If 2, SFX will play on every second letter etc")]
	public int SFXFrequency = 2;
	public bool usePitchVariation = true;
	public Vector2 volumeRandomisationRange = new Vector2(0.3f, 0.4f);
	[Range(0.05f, 0.5f)]
	public float pitchChangeMultiplier = 0.1f;

	[Space]
	public float sfxDelay = 0.015f;
	public float specialCharacterDelay = 0.015f;
	public float highlightedWordDelay = 0.05f;
	public float interruptDelay = 0.015f;
	bool wordDelayActive = false;

	//UI HACKS
	[Header("UI Hacks: ")]
	public GameObject takoUI;
	public GameObject temUI;
	public Image npcSprite;

	[Header("Extra Settings For Cutscene Compatibility")]
	public bool openingCutscene = false;
	public bool useCharacterDialogueBoxes = false;
	public bool useCustomAnimator = false;
	public bool useTriggerAudioOnly = false;
	public TMP_Text customCutsceneTextBox;
	public bool useDialogueSound = true;
	public float onScreenTextDelay = 3f;
	bool cutscene = false;
	float durationOfText;
	bool startTiming = false;

	//These are update with dialogue triggers
	List<DialogueEntry> currentDialogueEntries;
	AudioClip nonPlayerDialogueAudio;

	[Header("Debugs/References: ")]
	bool dialogueInProgress = false;
	public bool dialogueActive;
	TMP_Text activeText;
	int index;
	bool canTalk;
	bool interruptDialogue = false;

	[Header("Testing/Hacks: ")]
	public bool testUsingSFXDelay;
	public bool standAloneCutsceneControlls;
	bool triggerDialogueFinishedEvent;
	public event System.Action onDialogueFinishEvent;
	public event System.Action onDialogueStartedEvent;
	public GameObject sceneTriggerHack;
	public bool battleSequence;
	public AudioClip takoDialogueSFXHack;
	public AudioClip temDialogueSFXHack;
	public Animator takoDialogueAnimator;
	public Animator temDialogueAnimator;
	int dialogueIndexCheck;
	public GameObject tutorialPrompt;
	public GameObject normalPrompt;
	public GameObject npcNormalPrompt;

	private void Awake()
    {
		instance = this;

		//Make sure all dialogue is cleared on fresh game start
		ResetAndClearDialogue();

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

	public void StartCutsceneDialogue(List<DialogueEntry> dialogueEntriesRecieved, bool triggerDialogueFinished)
    {
		index = 0;
		cutscene = true;
		currentDialogueEntries = dialogueEntriesRecieved;
		triggerDialogueFinishedEvent = triggerDialogueFinished;

		if (customCutsceneTextBox != null)
		{
			activeText = customCutsceneTextBox;
			activeText.text = "";
		}

		//timer for timeline reference
		durationOfText = 0;
		startTiming = true;

		StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
	}

    private void Update()
    {
        if(startTiming)
        {
			durationOfText += Time.deltaTime;
        }

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
		//if dialogue in progress - skip to end
		if (dialogueInProgress)
		{
			if (cutscene)
			{
				//Skip Cutscene
				Debug.Log("Skip");
			}
			else
			{
				StopAllCoroutines();
				GetDialogueBox();

				activeText.text = currentDialogueEntries[index].dialogue;

				OnCurrentDialogueFinished();
				return;
			}
		}

		//Turn Off Continue dialogue prompt
		TriggerDialoguePrompt(false);

		if(!dialogueFinished)
        {
			//Play Interact Sound
			if (!cutscene)
			{
				if(interactAudioSource != null)
                {
					interactAudioSource.Play();
                }
				else
                {
                    audioManager.clip = interactSound;
                    audioManager.volume = 0.1f;
                    audioManager.pitch = 1;
					audioManager.Play();
				}
			}

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

		//Set Dialogue Speed
		if (currentDialogueEntries[index].useCustomDialogueSpeed)
			dialogueSpeed = currentDialogueEntries[index].customDelayTime;
		else
			dialogueSpeed = defaultDialogueSpeed;

		yield return new WaitForSeconds(0.15f);

		if(!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			GetDialogueBox();

			//Play animation
			if (animator != null)
            {
				if (currentDialogueEntries[index].talkAnimation != null)
					animator.Play(currentDialogueEntries[index].talkAnimation.name);
				else if(currentDialogueEntries[index].placeholderSprite != null)
                {
					//Placeholder for Ugly Duckling
					if (currentDialogueEntries[index].temDialogue)
					{
						temUI.GetComponent<Image>().sprite = currentDialogueEntries[index].placeholderSprite;
					}
					else
					{
						if (npcSprite != null)
							npcSprite.sprite = currentDialogueEntries[index].placeholderSprite;
					}
                }
				else
					Debug.Log("No Talk Animation Configured");
            }
		}
		else
			activeText.text = "";

		//dialogueInProgress = true;

		//Print dialogue letter by letter
		foreach (char letter in currentDialogue.ToCharArray())
        {
			//If special word character found
			if (letter.ToString() == "<" || letter.ToString() == ">")
            {
				//if at the end - reset to default speed
				if (wordDelayActive)
                {
					wordDelayActive = false;
					dialogueSpeed = defaultDialogueSpeed;
					continue;
                }
				else
                {
					//adjust speed for special word
					wordDelayActive = true;
					dialogueSpeed = highlightedWordDelay;
					continue;
                }
            }

			//Print Letter
			yield return new WaitForSeconds(dialogueSpeed);
			activeText.text += letter;

			//Custom OneShot Audio
			if (!dialogueSFXWasTempDisabled && letter.ToString() == "*")
			{
				if (currentDialogueEntries[index].oneOffCustomSFX != null)
				{
					useDialogueSound = false;
					customSFXAudioSource.PlayOneShot(currentDialogueEntries[index].oneOffCustomSFX);
					dialogueSFXWasTempDisabled = true;
				}
			}

			//Handle Dialogue SFX------------------------------
			if (useDialogueSound)
			{
				//Adjust Pitch/Volume
				if (usePitchVariation)
				{
					audioManager.volume = Random.Range(volumeRandomisationRange.x, volumeRandomisationRange.y);
					audioManager.pitch = Random.Range(1 - pitchChangeMultiplier, 1 + pitchChangeMultiplier);
				}

				//Play Dialogue SFX - might need to seperate this from the main coroutine
				dialogueIndexCheck += 1;

				//Debug.Log(dialogueIndexCheck % 3);

				if(useCustomSFXFrequency)
                {
					if (dialogueIndexCheck % SFXFrequency == 0)
					{
						if (testUsingSFXDelay)
						{
							yield return new WaitForSeconds(sfxDelay);
							audioManager.Play();

							//BUG: This only delays when it starts - not the frequency
							//StartCoroutine(PlayDialogueSoundCoroutine());
						}
						else
							audioManager.Play();
					}
				}
				else
                {
					if (testUsingSFXDelay)
					{
						yield return new WaitForSeconds(sfxDelay);
						audioManager.Play();
					}
					else
						audioManager.Play();
				}
			}
        }

		OnCurrentDialogueFinished();
	}

	void OnCurrentDialogueFinished()
	{
		dialogueInProgress = false;
		if (dialogueSFXWasTempDisabled)
			useDialogueSound = true;

		if(!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			//Stop Animation / (Idle)
			if (animator != null)
			{
				if (currentDialogueEntries[index].idleAnimation != null)
					animator.Play(currentDialogueEntries[index].idleAnimation.name);
				else
					Debug.Log("No Idle Animation Configured");
			}
			else
				Debug.Log("Animator Reference Missing");
		}

		//Continue dialogue prompt
		if (!cutscene)
		{
			if (!cutscene)
				TriggerDialoguePrompt(true);
		}

		//Progress Dialogue Position
		index++;

		//Auto Start Next Dialogue
		if (cutscene || interruptDialogue)
		{
			interruptDialogue = false;
			if (cutscene && !openingCutscene)
				Invoke("ContinueDialogue", onScreenTextDelay);
			else
				ContinueDialogue();
		}
	}

	void TriggerDialoguePrompt(bool setActive)
    {
		//Turn On/Off Continue dialogue prompt
		if (index <= 1 && tutorialPrompt != null)
        {
			tutorialPrompt.SetActive(setActive);

			//Hack to stop this from turning on again
			if (!setActive)
				tutorialPrompt = null;
        }
		else
		{
			if(currentDialogueEntries.Count > index)
            {
				//If NPC
				if (!currentDialogueEntries[index].takoDialogue && !currentDialogueEntries[index].temDialogue)
				{
					if (npcNormalPrompt != null)
						npcNormalPrompt.SetActive(setActive);
					else
						Debug.Log("Missing NPC Continue Dialogue Prompt");
				}
				else
				{
					if (normalPrompt != null)
						normalPrompt.SetActive(setActive);
					else
						Debug.Log("Missing Continue Dialogue Prompt");
				}
			}
		}
	}

	void EndDialogue()
    {
		if(cutscene)
        {
			startTiming = false;
			Debug.Log("DONE! Duration: " + durationOfText);
        }

		if(!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			//When dialogue ends, update the game state
			//if(GameManager.instance != null)
				//GameManager.instance.UpdateDialogueGameState(false);

			//canTalk = false;
			ResetAndClearDialogue();

			//HACK: Using this to trigger the scene transition for now
			if (triggerDialogueFinishedEvent)
				onDialogueFinishEvent?.Invoke();
		}

		dialogueActive = false;
		dialogueFinished = false;
	}

	void ResetAndClearDialogue()
	{
		//Clear all text and dialogue entries - THIS WAS CLEARING THE DIALOGUE TRIGGER LIST?
		//if(currentDialogueEntries != null)
		//currentDialogueEntries.Clear();

		/*dialogueActive = false;
		dialogueFinished = false;*/

		if (playerDialogueGO != null)
		{
			playerDialogueGO.GetComponentInChildren<TMP_Text>().text = "";
			playerDialogueGO.SetActive(false);

			//Hide Dialogue
			//HideDialogueBox();
		}

		if (npcDialogueGO != null)
		{
			npcDialogueGO.GetComponentInChildren<TMP_Text>().text = "";
			npcDialogueGO.SetActive(false);
		}

		if (narrativeDialogueGO != null)
			narrativeDialogueGO.SetActive(false);
	}

	void GetDialogueBox()
    {
		if (currentDialogueEntries[index].takoDialogue || currentDialogueEntries[index].temDialogue)
		{
			//Turn of Other Text Boxes
			if (narrativeDialogueGO != null)
				narrativeDialogueGO.SetActive(false);

			if (npcDialogueGO != null)
				npcDialogueGO.SetActive(false);

			//If player is talking show player dialogue box
			playerDialogueGO.SetActive(true);

			//Get Text - Hack work around with Tutorial Prompt
			if (tutorialPrompt != null)
				activeText = playerDialogueGO.transform.GetChild(1).GetComponent<TMP_Text>();
			else
				activeText = playerDialogueGO.GetComponentInChildren<TMP_Text>();

			//Is this new dialogue Tako or Tem
			bool isTako;
			if (currentDialogueEntries[index].takoDialogue)
				isTako = true;
			else
				isTako = false;

			SetupCharacterDialogueProfile(isTako);
		}
		else if(currentDialogueEntries[index].narrationDialogue)
        {
			//Turn of Other Text Boxes
			if (playerDialogueGO != null)
				playerDialogueGO.SetActive(false);

			if (npcDialogueGO != null)
				npcDialogueGO.SetActive(false);

			//Otherwise show NPC dialogue box
			narrativeDialogueGO.SetActive(true);
			activeText = narrativeDialogueGO.GetComponentInChildren<TMP_Text>();

			//Get SFX
			if (currentDialogueEntries[index].customAudioSFX != null)
			{
				audioManager.clip = currentDialogueEntries[index].customAudioSFX;
			}
			else
				audioManager.clip = nonPlayerDialogueAudio;
		}
		else
		{
			//Turn of Other Text Boxes
			if (narrativeDialogueGO != null)
				narrativeDialogueGO.SetActive(false);

			if (playerDialogueGO != null)
				playerDialogueGO.SetActive(false);

			//Otherwise show NPC dialogue box
			npcDialogueGO.SetActive(true);
			activeText = npcDialogueGO.GetComponentInChildren<TMP_Text>();

			//Get NPC SFX
			if (currentDialogueEntries[index].customAudioSFX != null)
			{
				audioManager.clip = currentDialogueEntries[index].customAudioSFX;
			}
			else
				audioManager.clip = nonPlayerDialogueAudio;
		}

		//Reset text
		activeText.text = "";
	}

	void SetupCharacterDialogueProfile(bool isTako)
    {
		if (takoUI != null)
			takoUI.SetActive(isTako);
		else
			Debug.Log("TakoUI Missing");
		
		if(temUI != null)
			temUI.SetActive(!isTako);
		else
			Debug.Log("TemUI Missing");

		//Get player SFX
		if (!useTriggerAudioOnly)
		{
			if (isTako)
				audioManager.clip = takoDialogueSFXHack;
			else
				audioManager.clip = temDialogueSFXHack;
		}
		else
			audioManager.clip = nonPlayerDialogueAudio;

		//Get Animator
		if (!useCustomAnimator)
		{
			//if (battleSequence)
			{
				if (isTako)
					animator = takoDialogueAnimator;
				else
					animator = temDialogueAnimator;
			}
		}
	}
}

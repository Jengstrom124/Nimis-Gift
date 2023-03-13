using UnityEngine;
using UnityEngine.Events;

//For each dialogue entry
[System.Serializable]
public class DialogueEntry
{
	//The dialogue
	[TextArea(3, 10)]
	public string dialogue;

	//The animations
	public AnimationClip talkAnimation;
	public AnimationClip idleAnimation;
	public Sprite placeholderSprite;

	//As we want different text boxes for player vs npc, check whether dialogue is player or npc
	//public bool playerDialogue;
	[Space]
	public bool takoDialogue;
	public bool temDialogue;
	public bool narrationDialogue = false;

	[Header("Custom Options")]
	public bool useCustomDialogueSpeed = false;
	public float customDelayTime;
	public AudioClip customAudioSFX;
	public AudioClip oneOffCustomSFX;

    //SFX
    //public AudioClip audio;
}

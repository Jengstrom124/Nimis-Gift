using UnityEngine;
using UnityEngine.Events;

//For each dialogue entry
[System.Serializable]
public class DialogueEntry
{
	//The dialogue
	[TextArea(3, 10)]
	public string dialogue;

	[Header("Custom Options")]
	public bool useCustomDialogueSpeed = false;
	public float customDelayTime;
	public AudioClip customAudioSFX;
	public AudioClip oneOffCustomSFX;

    //SFX
    //public AudioClip audio;
}

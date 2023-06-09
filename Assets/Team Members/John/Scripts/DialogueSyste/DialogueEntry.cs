using UnityEngine;

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
	public bool endingDialogueHack = false;

    //SFX
    //public AudioClip audio;
}

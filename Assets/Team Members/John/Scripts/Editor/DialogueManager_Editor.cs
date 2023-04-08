using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueManager))]

public class DialogueManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("End Dialogue Sequence"))
		{
			(target as DialogueManager)?.EndDialogue();
		}
	}
}

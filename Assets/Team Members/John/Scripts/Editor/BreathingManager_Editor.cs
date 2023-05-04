using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NimiBreathingManager))]

public class BreathingManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Start Breathing Exercise"))
		{
			(target as NimiBreathingManager)?.BeginBreathingExercise(0);
		}
	}
}

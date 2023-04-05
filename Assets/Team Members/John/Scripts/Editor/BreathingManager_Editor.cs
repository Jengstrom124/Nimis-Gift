using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BreathingManager))]

public class BreathingManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Start Breathing Exercise"))
		{
			(target as BreathingManager)?.BeginBreathingExercise();
		}
	}
}

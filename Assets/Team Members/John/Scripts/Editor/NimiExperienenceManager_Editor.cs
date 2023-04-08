using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NimiExperienceManager))]

public class NimiExperienenceManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Upgrade Phase 1 Environment Debug (No Dialouge)"))
		{
			(target as NimiExperienceManager)?.UpgradeEnvironmentDebug();
		}

		if (GUILayout.Button("Upgrade Phase 1 Environment Sequence Debug"))
		{
			(target as NimiExperienceManager)?.PostFirstPhaseBreathing();
		}
	}
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NimiExperienceManager))]

public class NimiExperienenceManager_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Upgrade Environment Debug"))
		{
			(target as NimiExperienceManager)?.UpgradeEnvironmentDebug();
		}
	}
}

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stage))]
public class StageEditor : Editor
{
	public override void OnInspectorGUI()
	{
		Stage stage = target as Stage;
		stage.ActionGUI ();
		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}


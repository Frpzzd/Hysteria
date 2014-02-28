using System;
using UnityEditor;
using UnityEngine;
using DanmakuEngine.Actions;

[CustomEditor(typeof(ActionStage))]
public class StageEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SerializedObject stage = serializedObject;

		SerializedProperty theme = stage.FindProperty ("theme");
		SerializedProperty bonus = stage.FindProperty ("bonus");
		SerializedProperty actions = stage.FindProperty ("actions");

		EditorGUILayout.PropertyField (theme);
		EditorGUILayout.PropertyField (bonus);
		DanmakuEditorUtils.ActionGroupField (actions, this, true);
		stage.ApplyModifiedProperties ();
	}
	
	void OnSceneGUI()
	{
		ActionStage stage = target as ActionStage;
		Handles.DrawCamera (new Rect (0, 0, Screen.width, Screen.height), Camera.current);
		if(stage.actions != null)
		{
			for(int i = 0; i < stage.actions.Length; i++)
			{
				stage.actions[i].Initialize(stage);
				stage.actions[i].DrawHandles((i == 0) ? null : stage.actions[i - 1], Color.cyan);
			}
		}
	}
}


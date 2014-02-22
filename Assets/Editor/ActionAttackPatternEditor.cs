using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Actions;

[CustomEditor(typeof(ActionAttackPattern))]
public class ActionAttackPatternEditor : Editor
{
	private SerializedObject aap;

	public override void OnInspectorGUI ()
	{
		aap = serializedObject;
		SerializedProperty movementActions = aap.FindProperty ("actions");
		SerializedProperty fireTags = aap.FindProperty ("fireTags");
		SerializedProperty bulletTags = aap.FindProperty ("bulletTags");

		DanmakuEditorUtils.AttackPatternPropertiesGUI (aap);
		if(EditorGUILayout.PropertyField (movementActions, new GUIContent("Movement Pattern")))
		{
			CommonActionDrawer.attackPattern = target as ActionAttackPattern;
			DanmakuEditorUtils.ActionGroupField(movementActions, this, true);
		}
		if(EditorGUILayout.PropertyField (fireTags))
		{
			CommonActionDrawer.attackPattern = target as ActionAttackPattern;
			DanmakuEditorUtils.TagGroupField(fireTags, this, false);
		}
		if(EditorGUILayout.PropertyField (bulletTags))
		{
			CommonActionDrawer.attackPattern = target as ActionAttackPattern;
			DanmakuEditorUtils.TagGroupField(bulletTags, this, false);
		}
		aap.ApplyModifiedProperties ();
	}
}


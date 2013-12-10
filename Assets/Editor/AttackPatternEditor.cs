using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackPattern))]
public class AttackPatternEditor : Editor 
{
	public override void OnInspectorGUI() 
	{
		SerializedProperty bossPattern = serializedObject.FindProperty("bossPattern");
		bossPattern.boolValue = EditorGUILayout.BeginToggleGroup("Boss Pattern", bossPattern.boolValue);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("bpName"), new GUIContent("Name"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("survival"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("timeout"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("bonus"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("bonusPerSecond"));
		EditorGUI.indentLevel--;
		EditorGUILayout.EndToggleGroup();
		serializedObject.ApplyModifiedProperties();
	}
}
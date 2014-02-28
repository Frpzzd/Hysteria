using System;
using UnityEditor;
using UnityEngine;
using DanmakuEngine.Actions;

[CustomPropertyDrawer(typeof(BulletTag))]
public class BulletTagDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 16f;
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty name = property.FindPropertyRelative ("name");
		SerializedProperty actions = property.FindPropertyRelative ("actions");
		SerializedProperty prefab = property.FindPropertyRelative ("prefab");
		SerializedProperty speed = property.FindPropertyRelative ("speed");
		SerializedProperty overwriteColor = property.FindPropertyRelative ("overwriteColor");
		SerializedProperty newColor = property.FindPropertyRelative ("newColor");
		EditorGUI.LabelField (position, "Bullet Tag: ", name.stringValue);
		EditorGUILayout.PropertyField (prefab);
		EditorGUILayout.PropertyField (name);
		EditorGUILayout.PropertyField (speed);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(overwriteColor);
		if(overwriteColor.boolValue)
		{
			EditorGUILayout.PropertyField(newColor, new GUIContent(""));
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel++;
		DanmakuEditorUtils.ActionGroupField (actions, null, true);
	}
}


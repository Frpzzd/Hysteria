using System;
using UnityEditor;
using UnityEngine;
using DanmakuEngine.Actions;

[CustomPropertyDrawer(typeof(FireTag))]
public class FireTagDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 16f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty name = property.FindPropertyRelative ("name");
		SerializedProperty runAtStart = property.FindPropertyRelative ("runAtStart");
		SerializedProperty loop = property.FindPropertyRelative ("loopUntilEnd");
		SerializedProperty minimumRank = property.FindPropertyRelative ("minimumRank");
		SerializedProperty actions = property.FindPropertyRelative ("actions");
		SerializedProperty loopWait = property.FindPropertyRelative ("wait");
		EditorGUI.LabelField (position, "Fire Tag: ", name.stringValue);
		EditorGUILayout.PropertyField (name);
		EditorGUILayout.PropertyField (minimumRank);
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PropertyField (runAtStart);
		EditorGUILayout.PropertyField (loop);
		EditorGUILayout.EndHorizontal ();
		if(loop.boolValue)
		{
			EditorGUILayout.PropertyField(loopWait);
		}
		EditorGUI.indentLevel++;
		DanmakuEditorUtils.ActionGroupField (actions, null, false);
	}
}


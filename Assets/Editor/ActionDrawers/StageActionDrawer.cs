using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Actions;

[CustomPropertyDrawer(typeof(ActionStage.Action))]
public class StageActionDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 0f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUILayout.LabelField ("StageAction");
		SerializedProperty type = property.FindPropertyRelative ("type");
		SerializedProperty wait = property.FindPropertyRelative ("wait");
		SerializedProperty repeat = property.FindPropertyRelative ("repeat");
		SerializedProperty usingRepeat = property.FindPropertyRelative ("usingRepeat");
		SerializedProperty representation = property.FindPropertyRelative ("representation");
		SerializedProperty prefab = property.FindPropertyRelative ("prefab");
		SerializedProperty location = property.FindPropertyRelative ("location");
		SerializedProperty us = property.FindPropertyRelative ("useSequence");
		SerializedProperty mmx = property.FindPropertyRelative ("mirrorMovementX");
		SerializedProperty mmy = property.FindPropertyRelative ("mirrorMovementY");
		ActionStage.Action.Type st = CommonActionDrawer.EnumChoice<ActionStage.Action.Type> (type, "Type");
		representation.stringValue = st.ToString ();
		usingRepeat.boolValue = st == ActionStage.Action.Type.Repeat;
		switch(st)
		{
			case ActionStage.Action.Type.Wait:
				EditorGUILayout.PropertyField(wait);
				break;
			case ActionStage.Action.Type.Repeat:
				EditorGUILayout.PropertyField(repeat);
				break;
			case ActionStage.Action.Type.SpawnEnemy:
				EditorGUILayout.PropertyField(prefab);
				EditorGUILayout.PropertyField(location);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(mmx);
				EditorGUILayout.PropertyField(mmy);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.PropertyField(us);
				break;
		}
	}
}


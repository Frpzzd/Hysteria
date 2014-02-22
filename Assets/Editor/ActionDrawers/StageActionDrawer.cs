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
		EditorGUILayout.LabelField ("MovementAction");
		SerializedProperty type = property.FindPropertyRelative ("type");
		SerializedProperty wait = property.FindPropertyRelative ("wait");
		SerializedProperty repeat = property.FindPropertyRelative ("repeat");
		SerializedProperty usingRepeat = property.FindPropertyRelative ("usingRepeat");
		SerializedProperty representation = property.FindPropertyRelative ("representation");
		SerializedProperty prefab = property.FindPropertyRelative ("prefab");
		SerializedProperty location = property.FindPropertyRelative ("location");
		SerializedProperty us = property.FindPropertyRelative ("useSequence");
		SerializedProperty mm = property.FindPropertyRelative ("mirrorMovement");
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
				EditorGUILayout.PropertyField(us);
				EditorGUILayout.PropertyField(mm);
				EditorGUILayout.EndHorizontal();
				break;
		}
	}
}


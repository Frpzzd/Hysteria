using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Actions;

[CustomPropertyDrawer(typeof(MovementAction))]
public class MovementActionDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 0f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUILayout.LabelField ("MovementAction");
		SerializedProperty type = property.FindPropertyRelative ("type");
		SerializedProperty locationType = property.FindPropertyRelative ("locationType");
		SerializedProperty targetLocation = property.FindPropertyRelative ("targetLocation");
		SerializedProperty wait = property.FindPropertyRelative ("wait");
		SerializedProperty repeat = property.FindPropertyRelative ("repeat");
		SerializedProperty usingRepeat = property.FindPropertyRelative ("usingRepeat");
		SerializedProperty representation = property.FindPropertyRelative ("representation");
		SerializedProperty control1 = property.FindPropertyRelative ("control1");
		SerializedProperty control2 = property.FindPropertyRelative ("control2");
		MovementAction.Type t = CommonActionDrawer.EnumChoice<MovementAction.Type>(type, "Type");
		representation.stringValue = t.ToString();
		usingRepeat.boolValue = t == MovementAction.Type.Repeat;
		if(t != MovementAction.Type.Wait  && t != MovementAction.Type.Repeat)
		{
			CommonActionDrawer.EnumChoice<MovementAction.LocationType>(locationType, "Location Type");
			EditorGUILayout.PropertyField(targetLocation);
		}
		switch(t)
		{
			case MovementAction.Type.Wait:
			case MovementAction.Type.Linear:
				EditorGUILayout.PropertyField(wait);
				break;
			case MovementAction.Type.Curve:
				EditorGUILayout.PropertyField(wait);
				EditorGUILayout.PropertyField(control1);
				EditorGUILayout.PropertyField(control2);
				break;
			case MovementAction.Type.Repeat:
				EditorGUILayout.PropertyField(repeat);
				break;
		}
	}
}


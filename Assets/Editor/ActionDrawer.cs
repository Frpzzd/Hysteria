using System;
using UnityEditor;

[CustomPropertyDrawer(typeof(Action))]
public class ActionDrawer : PropertyDrawer
{
	public override void OnGUI (UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label)
	{
		base.OnGUI (position, property, label);
		EditorGUI.BeginProperty (position, label, property);
		EditorGUI.EndProperty ();
	}
}


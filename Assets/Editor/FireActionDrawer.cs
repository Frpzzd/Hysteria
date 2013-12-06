using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FireAction))]
public class FireActionDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "Action");
		if(property.isExpanded)
		{
			EditorGUI.indentLevel++;
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
	}
}


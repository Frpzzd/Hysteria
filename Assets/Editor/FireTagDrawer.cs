using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FireTag))]
public class FireTagDrawer : PropertyDrawer
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "Fire Tag");
		if(property.isExpanded)
		{
			EditorGUI.indentLevel++;
			EditorUtil.ArrayGUI(property, "actions");
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndProperty();
	}
}


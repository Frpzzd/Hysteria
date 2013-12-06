using System;
using UnityEngine;
using UnityEditor;

public class EditorUtil
{
	public const int IndentSpacing = 10;

	public static void ArrayGUI(SerializedObject obj, string name)
	{
		ArrayGUIImpl(obj.FindProperty(name));
	}

	public static void ArrayGUI(SerializedProperty prop, string name)
	{
		ArrayGUIImpl(prop.FindPropertyRelative(name));
	}

	private static void ArrayGUIImpl(SerializedProperty sp)
	{
		if(sp.isArray)
		{
			int size = sp.arraySize;
			int c = size;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(sp);
			if(GUILayout.Button("Collapse All"))
			{
				
			}
			EditorGUILayout.EndHorizontal();
			if(sp.isExpanded)
			{
				int removeIndex = -1;
				int moveIndex = -1;
				int moveDir = 0;
				EditorGUI.indentLevel++;

				for (int i=0; i < size; i++) {
					EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(i), true);
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUI.indentLevel * IndentSpacing);
					GUI.enabled = (i > 0);
					if(GUILayout.Button("Up"))
					{
						moveIndex = i;
						moveDir = -1;
					}
					GUI.enabled = (i < size - 1);
					if(GUILayout.Button("Down"))
					{
						moveIndex = i;
						moveDir = 1;
					}
					GUI.enabled = true;
					if(GUILayout.Button("Remove"))
					{
						removeIndex = i;
					}
					EditorGUILayout.EndHorizontal();
				}
				
				if(moveIndex > 0)
				{
				}
				else if(moveIndex < 0)
				{
				}
				
				if(removeIndex >= 0)
				{
					if(sp.GetArrayElementAtIndex(removeIndex) != null)
					{
						sp.DeleteArrayElementAtIndex(removeIndex);
					}
					sp.DeleteArrayElementAtIndex(removeIndex);
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(EditorGUI.indentLevel * IndentSpacing);
				if(GUILayout.Button("Add"))
				{
					c++;
				}
				if(GUILayout.Button("Clear All"))
				{
					sp.ClearArray();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}
			if (c != size)
			{
				sp.arraySize = c;
			}
		}
	}
}


using System;
using UnityEngine;
using UnityEditor;

public class EditorUtil
{
	public const int IndentSpacing = 10;

	public static void ArrayGUI(SerializedObject obj, string name)
	{
		ArrayGUIImpl(obj.FindProperty(name), null);
	}
	
	public static void ArrayGUI(SerializedProperty prop, string name)
	{
		ArrayGUIImpl(prop.FindPropertyRelative(name), null);
	}

	public static void ArrayGUI(SerializedObject obj, string name, string label)
	{
		ArrayGUIImpl(obj.FindProperty(name), label);
	}

	public static void ArrayGUI(SerializedProperty prop, string name, string label)
	{
		ArrayGUIImpl(prop.FindPropertyRelative(name), label);
	}

	private static void ArrayGUIImpl(SerializedProperty sp, string label)
	{
		if(sp.isArray)
		{
			int size = sp.arraySize;
			int c = size;
			EditorGUILayout.BeginHorizontal();
			if(label == null)
			{
				EditorGUILayout.PropertyField(sp);
			}
			else
			{
				EditorGUILayout.PropertyField(sp, new GUIContent(label));
			}
			if(GUILayout.Button("+"))
			{
				c++;
			}
			if(GUILayout.Button("Collapse All"))
			{
				
			}
			if(GUILayout.Button("Clear"))
			{
				sp.ClearArray();
			}
			EditorGUILayout.EndHorizontal();
			if(sp.isExpanded)
			{
				int removeIndex = -1;
				int moveIndex = -1;
				int moveDir = 0;
				EditorGUI.indentLevel++;
				for (int i = 0; i < size; i++) 
				{
					GUILayout.Space(EditorGUI.indentLevel * IndentSpacing);
					EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(i), true);
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
					if(GUILayout.Button("X"))
					{
						removeIndex = i;
					}
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
				EditorGUI.indentLevel--;
			}
			if (c != size)
			{
				sp.arraySize = c;
			}	
		}
	}
}


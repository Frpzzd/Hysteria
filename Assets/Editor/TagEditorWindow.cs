using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TagEditorWindow : EditorWindow
{
	private static TagEditorWindow _instance;
	public static TagEditorWindow instance
	{
		get 
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<TagEditorWindow>("Tag");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}

	private SerializedProperty tag;
	private Editor origin;
	private Vector2 scroll;

	[MenuItem("Window/Tag Editor")]
	public static void ShowWindow()
	{
		_instance = EditorWindow.GetWindow<TagEditorWindow> ("Tag");
	}

    void OnGUI()
    {
		if(tag != null)
		{
			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUILayout.PropertyField(tag, true);
			EditorGUILayout.EndScrollView();
		}
		if(origin != null)
		{
			origin.Repaint();
		}
		if(GUI.changed)
		{
			tag.serializedObject.ApplyModifiedProperties();
		}
    }

	public static void SetTag(SerializedProperty tag, Editor origin)
	{
		if(tag != null && origin != null)
		{
			ShowWindow();
			instance.tag = tag;
			instance.origin = origin;
			instance.Repaint();
		}
	}
}
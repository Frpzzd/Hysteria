using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ActionEditorWindow : EditorWindow
{
	private static ActionEditorWindow _instance;
	public static ActionEditorWindow instance
	{
		get 
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<ActionEditorWindow>("Actions");
			}
			return _instance;
		}
		
		set
		{
			_instance = value;
		}
	}
	
	private SerializedProperty action;
	private Editor origin;
	private Vector2 scroll;
	
	[MenuItem("Window/Action Editor")]
	public static void ShowWindow()
	{
		_instance = EditorWindow.GetWindow<ActionEditorWindow> ("Actions");
	}
	
	void OnGUI()
	{
		if(action != null)
		{
			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUILayout.PropertyField(action, true);
			EditorGUILayout.EndScrollView();
		}
		if(origin != null)
		{
			origin.Repaint();
		}
		if(GUI.changed)
		{
			action.serializedObject.ApplyModifiedProperties();
		}
	}
	
	public static void SetAction(SerializedProperty action, Editor origin)
	{
		if(action != null)
		{
			ShowWindow();
			instance.action = action;
			instance.origin = origin;
			instance.Repaint();
		}
	}
}
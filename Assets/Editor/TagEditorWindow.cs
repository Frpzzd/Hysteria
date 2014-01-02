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
				_instance = EditorWindow.GetWindow<TagEditorWindow>("Actions");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}
    private Vector2 scroll;
	private static IActionGroup actionGroup;
	public static object[] parameters;
	public static bool editing = false;
	public static UnityEngine.Object container;

	[MenuItem("Window/ActionGroup Editor")]
	public static void ShowWindow()
	{
		_instance = EditorWindow.GetWindow<TagEditorWindow> ("Actions");
	}

    void OnGUI()
    {
		if(!editing)
		{
			if(actionGroup != null)
			{
				scroll = EditorGUILayout.BeginScrollView(scroll);
				if (actionGroup != null)
				{
					actionGroup.ActionGUI(parameters);
				}
				EditorGUILayout.EndScrollView();
				if(GUI.changed)
				{
					EditorUtility.SetDirty(container);
				}
			}
		}
    }

	public static void SetActionGroup<T>(T group, UnityEngine.Object obj, params object[] param) where T : IActionGroup
	{
		if(group != null)
		{
			editing = true;
			parameters = param;
			actionGroup = group;
			container = obj;
			editing = false;
			instance.Repaint();
		}
		else
		{
			Debug.Log("Null ActionGroup");
		}
	}
}
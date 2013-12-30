using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ActionGroupEditorWindow : EditorWindow
{
	private static ActionGroupEditorWindow _instance;
	public static ActionGroupEditorWindow instance
	{
		get 
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<ActionGroupEditorWindow>("Actions");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}
    private Vector2 scroll;
	public static IActionGroup actionGroup;
	public static object[] parameters;
	public static bool editing = false;

	[MenuItem("Window/ActionGroup Editor")]
	public static void ShowWindow()
	{
		_instance = EditorWindow.GetWindow<ActionGroupEditorWindow> ("Actions");
		Debug.Log (_instance.position);
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
					EditorUtility.SetDirty((UnityEngine.Object)actionGroup);
				}
			}
		}
    }

	void Update()
	{
		instance = this;
	}
}
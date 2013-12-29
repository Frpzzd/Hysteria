using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackPatternActionEditorWindow : EditorWindow
{
	private static AttackPatternActionEditorWindow _instance;
	public static AttackPatternActionEditorWindow instance
	{
		get 
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<AttackPatternActionEditorWindow>("Actions");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}
    private Vector2 scroll;
	public ITag tag { get { return AttackPatternTagEditorWindow.tag; } }
	public AttackPattern attackPattern { get { return AttackPatternTagEditorWindow.attackPattern; } }

    void OnGUI()
    {
		instance = this;
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (tag != null)
        {
			tag.ActionGUI(attackPattern);
        }
        EditorGUILayout.EndScrollView();
    }

	void Update()
	{
		instance = this;
	}
}
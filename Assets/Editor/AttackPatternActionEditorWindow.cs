using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackPatternActionEditorWindow : EditorWindow
{
	public static AttackPatternActionEditorWindow instance;
    private Vector2 scroll;
	public Tag tag { get { return AttackPatternTagEditorWindow.tag; } }
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
		EditorUtils.RefreshReflection ();
	}
}
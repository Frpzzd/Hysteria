#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorUtils
{
    public static int NamedObjectPopup(string label, NamedObject[] objects, int selectedIndex, string nullName)
    {
        Dictionary<string, int> repeats = new Dictionary<string, int>();
        List<string> names = new List<string>(objects.Length);
        for (int i = 0; i < objects.Length; i++)
        {
            if (names.Contains(objects [i].Name))
            {
                if (objects [i].Name == null)
                {
                    objects [i].Name = nullName;
                }
                if (repeats.ContainsKey(objects [i].Name))
                {
                    repeats [objects [i].Name]++;
                } 
				else
                {
                    repeats [objects [i].Name] = 1;
                }
                names.Add(objects [i].Name + " " + (repeats [objects [i].Name] + 1));
            } else
            {
                names.Add(objects [i].Name);
            }
        }
		if(label == null)
		{
			selectedIndex = EditorGUILayout.Popup(selectedIndex, names.ToArray());
		}
		else
		{
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names.ToArray());
		}
        if (selectedIndex < 0 || selectedIndex >= objects.Length)
        {
            selectedIndex = -1;
        }
        return selectedIndex;
    }

	public static int UnityEngineObjectPopup(string label, UnityEngine.Object[] objects, int selectedIndex, string nullName)
	{
		Dictionary<string, int> repeats = new Dictionary<string, int>();
		List<string> names = new List<string>(objects.Length);
		for (int i = 0; i < objects.Length; i++)
		{
			if (names.Contains(objects [i].name))
			{
				if (objects [i].name == null)
				{
					objects [i].name = nullName;
				}
				if (repeats.ContainsKey(objects [i].name))
				{
					repeats [objects [i].name]++;
				} 
				else
				{
					repeats [objects [i].name] = 1;
				}
				names.Add(objects [i].name + " " + (repeats [objects [i].name] + 1));
			} else
			{
				names.Add(objects [i].name);
			}
		}
		if (selectedIndex < 0 || selectedIndex >= objects.Length)
		{
			selectedIndex = -1;
		}
		return selectedIndex;
	}

	public static void MoveRemoveAdd<T>(Vector3 moveRemove, List<T> list) where T : new()
    {
        if (moveRemove.y >= 0)
        {
			list.RemoveAt((int)moveRemove.y);
        }
        if (moveRemove.x >= 0)
        {
            int moveIndex = (int)moveRemove.x;
            if (moveRemove.z > 0)
            {
                Swap<T>(list, moveIndex, moveIndex + 1);
            }
            if (moveRemove.z < 0)
            {
                Swap<T>(list, moveIndex, moveIndex - 1);
            }
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10 * EditorGUI.indentLevel);
        if (GUILayout.Button("Add"))
        {
			list.Add(new T());
		}
        EditorGUILayout.EndHorizontal();
    }

	public static T[] ActionGUI<T, P> (T[] actions, bool zeroed, params object[] param) 
		where T : NestedAction<T, P>, new()
		where P : struct, IConvertible
	{
		List<T> actionList = new List<T> (actions);
		Vector3 moveRemove = new Vector3 (-1f, -1f, 0f);
		for(int i = 0; i < actionList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			Rect boundingRect = EditorGUILayout.BeginVertical();
			GUI.Box(boundingRect, "");
			EditorGUILayout.BeginHorizontal();
			actionList[i].foldout = EditorGUILayout.Foldout(actionList[i].foldout, actionList[i].ToString());

			moveRemove = UpDownRemoveButtons(moveRemove, actionList.Count, i, zeroed);
			EditorGUILayout.EndHorizontal();
			if (actionList[i].foldout)
			{
				EditorGUI.indentLevel++;
				actionList[i].ActionGUI(param);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		MoveRemoveAdd<T> (moveRemove, actionList);
		return actionList.ToArray ();
	}

	public static Vector3 UpDownRemoveButtons(Vector3 moveRemove, int count, int i)
	{
		return UpDownRemoveButtons(moveRemove, count, i, false);
	}
	
	public static Vector3 UpDownRemoveButtons(Vector3 moveRemove, int count, int i, bool zeroed)
	{
		GUI.enabled = (i > 0);
		if (GUILayout.Button('\u25B2'.ToString(), GUILayout.Width(22)))
		{
			moveRemove.x = i;       //Move Index
			moveRemove.z = -1f;     //Move Direction
		}
		GUI.enabled = (i < count - 1);
		if (GUILayout.Button('\u25BC'.ToString(), GUILayout.Width(22)))
		{
			moveRemove.x = i;       //Move Index
			moveRemove.z = 1f;      //Move Direction
		}
		GUI.enabled = (count > 1) || zeroed;
		if (GUILayout.Button("X", GUILayout.Width(22)))
		{
			moveRemove.y = i;       //Remove Index
		}
		GUI.enabled = true;
		return moveRemove;
	}

    public static void Swap<T>(List<T> list, int a, int b)
    {
        T temp = list [a];
        list [a] = list [b];
        list [b] = temp;
    }

	public static void ExpandCollapseButtons<T, P>(string label, T[] actions) 
		where T : NestedAction<T, P>
		where P : struct, IConvertible
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			for(int i = 0; i < actions.Length; i++)
			{
				actions[i].Expand(true);
			}
		}
		if (GUILayout.Button('\u2013'.ToString(), GUILayout.Width(20)))
		{
			for(int i = 0; i < actions.Length; i++)
			{
				actions[i].Collapse(true);
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}
#endif


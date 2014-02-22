using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DanmakuEditorUtils
{
	public static bool DuplicateCheck<T>(T target) where T : MonoBehaviour
	{
		GameObject gameObject = target.gameObject;
		GUIStyle centeredStyle = GUI.skin.GetStyle ("Label");
		centeredStyle.alignment = TextAnchor.MiddleCenter;
		T[] scripts = gameObject.GetComponents<T>();
		if(scripts == null || scripts.Length < 1)
		{
			EditorGUILayout.LabelField("Wut... this shouldn't be happening", centeredStyle);
		}
		else if(scripts.Length > 1)
		{
			EditorGUILayout.LabelField("Object has too many of this kind of script", centeredStyle);
			if(GUILayout.Button("Keep Me"))
			{
				foreach(T otherScript in gameObject.GetComponents<T>())
				{
					if(otherScript != target)
					{
						Editor.DestroyImmediate(otherScript, true);
					}
				}
				EditorUtility.SetDirty(gameObject);
			}
		}
		else
		{
			return true;
		}
		return false;
	}

	private static Color[] nestedColors = new Color[] {Color.white, Color.cyan, Color.magenta, Color.yellow, Color.grey, Color.green, Color.red};

	public static void ActionGroupField(SerializedProperty actions, Editor origin, bool zeroed)
	{
		ActionGroupField (actions, origin, zeroed, 0);
	}

	public static void ActionGroupField(SerializedProperty actions, Editor origin, bool zeroed, int i)
	{
		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		int c = actions.FindPropertyRelative ("Array.size").intValue;
		Color oldColor = GUI.color;
		GUI.color = nestedColors [i];
		for(int j = 0; j < c; j++)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty arrayElement = actions.GetArrayElementAtIndex(j);
			SerializedProperty nestedActions = arrayElement.FindPropertyRelative("nestedActions");
			SerializedProperty usingRepeat = arrayElement.FindPropertyRelative("usingRepeat");
			SerializedProperty representation = arrayElement.FindPropertyRelative("representation");
			GUILayout.Space(20*EditorGUI.indentLevel);
			if(GUILayout.Button (representation.stringValue))
			{
				ActionEditorWindow.SetAction(arrayElement, origin);
			}
			moveRemove = DanmakuEditorUtils.UpDownRemoveButtons(moveRemove, c, j, zeroed);
			EditorGUILayout.EndHorizontal();
			if(usingRepeat.boolValue)
			{
				if(i + 1 < nestedColors.Length)
				{
					EditorGUI.indentLevel++;
					ActionGroupField(nestedActions, origin, zeroed, i + 1);
					EditorGUI.indentLevel--;
				}
				else
				{
					GUIStyle centeredStyle = GUI.skin.GetStyle ("Label");
					centeredStyle.alignment = TextAnchor.MiddleCenter;
					EditorGUILayout.LabelField("Further nesting is not allowed.", centeredStyle);
				}
			}
		}
		if (moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			actions.DeleteArrayElementAtIndex(removeIndex);
		}
		if (moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if (moveRemove.z > 0)
			{
				actions.MoveArrayElement (moveIndex, moveIndex + 1);
			}
			if (moveRemove.z < 0)
			{
				actions.MoveArrayElement (moveIndex, moveIndex - 1);
			}
		}
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Space(20*EditorGUI.indentLevel);
		if (GUILayout.Button("Add"))
		{
			actions.InsertArrayElementAtIndex(c);
		}
		EditorGUILayout.EndHorizontal ();
		GUI.color = oldColor;
	}
	
	public static void TagGroupField(SerializedProperty tags, Editor origin, bool zeroed)
	{
		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		int c = tags.FindPropertyRelative ("Array.size").intValue;
		for(int i = 0; i < c; i++)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty arrayElement = tags.GetArrayElementAtIndex(i);		
			SerializedProperty name = arrayElement.FindPropertyRelative ("name");
			if(GUILayout.Button (name.stringValue))
			{
				TagEditorWindow.SetTag(arrayElement, origin);
			}
			moveRemove = RemoveButton(moveRemove, c, i, zeroed);
			EditorGUILayout.EndHorizontal();
		}
		if (moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			tags.DeleteArrayElementAtIndex(removeIndex);
		}
		if (moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if (moveRemove.z > 0)
			{
				tags.MoveArrayElement (moveIndex, moveIndex + 1);
			}
			if (moveRemove.z < 0)
			{
				tags.MoveArrayElement (moveIndex, moveIndex - 1);
			}
		}
		if (GUILayout.Button("Add"))
		{
			tags.InsertArrayElementAtIndex(c);
		}
	}

	public static void AttackPatternPropertiesGUI(SerializedObject attackPattern)
	{
		SerializedProperty name = attackPattern.FindProperty ("apName");
		SerializedProperty title = attackPattern.FindProperty ("apTitle");
		SerializedProperty health = attackPattern.FindProperty ("health");
		SerializedProperty timeout = attackPattern.FindProperty ("timeout");
		SerializedProperty bonus = attackPattern.FindProperty ("bonus");
		SerializedProperty survival = attackPattern.FindProperty ("survival");
		SerializedProperty drops = attackPattern.FindProperty ("drops");
		EditorGUILayout.PropertyField (name, new GUIContent ("Name"));
		EditorGUILayout.PropertyField (title, new GUIContent ("Title"));
		EditorGUILayout.PropertyField (health);
		EditorGUILayout.PropertyField (timeout);
		EditorGUILayout.PropertyField (bonus);
		EditorGUILayout.PropertyField (survival);
		EditorGUILayout.PropertyField (drops);
	}

	public static int NamedObjectPopup(string label, NamedObject[] objects, SerializedProperty prop, string nullName)
	{
		Dictionary<string, int> repeats = new Dictionary<string, int>();
		List<string> names = new List<string>(objects.Length);
		int selectedIndex = prop.intValue;
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
		prop.intValue = selectedIndex;
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
		moveRemove = RemoveButton (moveRemove, count, i, zeroed);
		return moveRemove;
	}

	public static Vector3 RemoveButton(Vector3 moveRemove, int count, int i)
	{
		return RemoveButton (moveRemove, count, i);
	}

	public static Vector3 RemoveButton(Vector3 moveRemove, int count, int i, bool zeroed)
	{
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
	
//	public static void ExpandCollapseButtons<T, P>(string label, T[] actions) 
//		where T : NestedAction<T, P>
//			where P : struct, IConvertible
//	{
//		EditorGUILayout.BeginHorizontal();
//		EditorGUILayout.LabelField(label);
//		if (GUILayout.Button("+", GUILayout.Width(20)))
//		{
//			for(int i = 0; i < actions.Length; i++)
//			{
//				actions[i].Expand(true);
//			}
//		}
//		if (GUILayout.Button('\u2013'.ToString(), GUILayout.Width(20)))
//		{
//			for(int i = 0; i < actions.Length; i++)
//			{
//				actions[i].Collapse(true);
//			}
//		}
//		EditorGUILayout.EndHorizontal();
//	}
}
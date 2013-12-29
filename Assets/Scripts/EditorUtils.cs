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
	public static Dictionary<Type, TypeDictionary> actionTypes;
	
	public class TypeDictionary
	{
		public Dictionary<String, Type> types;
		public string[] names;
		public Type baseType;
		
		public TypeDictionary(Type type)
		{
			baseType = type;
			types = new Dictionary<string, Type>();
			names = null;
		}

		public void Refresh()
		{
			types = new Dictionary<string, Type>();
			List<string> nameList = new List<string>();
			foreach(Type t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where (p => baseType.IsAssignableFrom(p) && !p.IsAbstract && !p.IsGenericType))
			{
				types.Add(ProcessName(t.ToString()), t);
				nameList.Add(ProcessName(t));
			}
			names = nameList.ToArray();
		}
		
		public Type this[int index]
		{
			get { return types [names[index]]; }
		}

		private static string ProcessName(string name)
		{
			string returnString = name;
			Regex capitalSpacing = new Regex("[A-Z]");
			Regex nestedClassing = new Regex (".*\\+");
			Regex generics = new Regex("\\`[0-9]*\\[.*\\]");
			returnString = generics.Replace (returnString, "");
			returnString = nestedClassing.Replace (returnString, "");
			returnString = capitalSpacing.Replace (returnString, " $0");
			return returnString;
		}

		public static string ProcessName(Type type)
		{
			return ProcessName(type.ToString());
		}
		
		public int Index(object obj)
		{
			string name = ProcessName (obj.GetType ());
			for(int i = 0; i < names.Length; i++)
			{
				if(name == names[i])
				{
					return i;
				}
			}
			return -1;
		}
	}

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
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names.ToArray());
		}
		else
		{
			selectedIndex = EditorGUILayout.Popup(selectedIndex, names.ToArray());
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

    public static void MoveRemoveAdd<T, P>(Vector3 moveRemove, List<T> list) where P : T, new()
    {
        if (moveRemove.y >= 0)
        {
			Debug.Log((int)moveRemove.y);
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
            list.Add(new P());
		}
        EditorGUILayout.EndHorizontal();
    }

	public static T[] ActionGUI<T, P>(T[] actions, bool zeroed, params object[] param) where T : Action where P : T, new()
	{
		List<T> actionList = new List<T> (actions);
		Vector3 moveRemove = new Vector3 (-1f, -1f, 0f);
		for(int i = 0; i < actionList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10 * EditorGUI.indentLevel);
			Rect boundingRect = EditorGUILayout.BeginVertical();
			GUI.Box(boundingRect, "");
			EditorGUILayout.BeginHorizontal();
			actionList[i].Foldout = EditorGUILayout.Foldout(actionList[i].Foldout, actionList[i].ToString());

			moveRemove = UpDownRemoveButtons(moveRemove, actionList.Count, i, zeroed);
			EditorGUILayout.EndHorizontal();
			if (actionList[i].Foldout)
			{
				EditorGUI.indentLevel++;
				Type tType = typeof(T);
				if(actionTypes == null)
				{
					actionTypes = new Dictionary<Type, TypeDictionary>();
				}
				if(!actionTypes.ContainsKey(tType))
				{
					actionTypes.Add(tType, new TypeDictionary(tType));
				}
				TypeDictionary typeStruct = actionTypes[tType];
				typeStruct.Refresh();
				Type actionType = typeStruct[EditorGUILayout.Popup(typeStruct.Index(actionList[i]), typeStruct.names)];
				if(actionType != actionList[i].GetType())
				{
					actionList[i] = (T)Activator.CreateInstance(actionType);
				}
				actionList[i].ActionGUI(param);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		MoveRemoveAdd<T, P> (moveRemove, actionList);
		return actionList.ToArray ();
	}
				//                    case(FireAction.Type.CallFireTag):
				//                        ac.fireTagIndex = EditorUtils.NamedObjectPopup("Fire Tag", bulletTags, ac.fireTagIndex, "Fire Tag");
				//                        EditorGUILayout.BeginHorizontal();
				//                        ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
				//                        if (!ac.passParam)
				//                        {
				//                            ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
				//                        }
				//                        EditorGUILayout.EndHorizontal();    
				//                        if (ac.passParam)
				//                        {
				//                            ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
				//                        }
				//                        break;
				//
				//                    case(FireAction.Type.Repeat):
				//                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
				//                        NestedFireActionsGUI(ac);
				//                        break;
				//                }

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

	public static void ExpandCollapseButtons(string label, Action[] actions)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label);
		if (GUILayout.Button("Expand All", GUILayout.Width(80)))
		{
			for(int i = 0; i < actions.Length; i++)
			{
				if(actions[i] is INestedAction)
				{
					(actions[i] as INestedAction).Expand(true);
				}
			}
		}
		if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
		{
			for(int i = 0; i < actions.Length; i++)
			{
				if(actions[i] is INestedAction)
				{
					(actions[i] as INestedAction).Collapse(true);
				}
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}
#endif


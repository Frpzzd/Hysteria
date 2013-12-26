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
	public static TypeStruct bulletActionTypes;
	public static TypeStruct fireActionTypes;
	
	public static void RefreshReflection()
	{
		bulletActionTypes = new TypeStruct (typeof(IBulletAction));
		fireActionTypes = new TypeStruct (typeof(IFireAction));
	}
	
	public class TypeStruct
	{
		public Dictionary<String, Type> types;
		public Dictionary<string, string> nameTranslationTable;
		public string[] names;
		
		public TypeStruct(Type baseType)
		{
			types = new Dictionary<string, Type>();
			nameTranslationTable = new Dictionary<string, string>();
			List<string> nameList = new List<string>();
			foreach(Type t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where (p => baseType.IsAssignableFrom(p) && !p.IsAbstract))
			{
				types.Add(t.ToString(), t);
				nameTranslationTable.Add(ProcessName(t), t.ToString());
				nameList.Add(ProcessName(t));
			}
			names = nameList.ToArray();
		}
		
		public Type this[int index]
		{
			get { return types [nameTranslationTable [names[index]]]; }
		}
		
		public static string ProcessName(Type type)
		{
			string returnString = type.ToString ().Replace("+","").Replace("FireAction","").Replace("BulletAction","").Replace("SharedAction","");
			Regex r = new Regex("[A-Z]");
			returnString = r.Replace (returnString, " $0");
			return returnString;
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
        selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names.ToArray());
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
			Debug.Log("Hello");
            int removeIndex = (int)moveRemove.y;
            list.RemoveAt(removeIndex);
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

	public static IBulletAction[] BulletActionGUI(IBulletAction[] bulletActions, AttackPattern attackPattern)
	{
		List<IBulletAction> actions = new List<IBulletAction>(bulletActions);
		
		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		for (int i = 0; i < actions.Count; i++)
		{
			Rect boundingRect = EditorGUILayout.BeginVertical();
			GUI.Box(boundingRect, "");
			EditorGUILayout.BeginHorizontal();
			actions[i].Foldout = EditorGUILayout.Foldout(actions[i].Foldout, "");

			moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i, true);
			EditorGUILayout.EndHorizontal();
			if (actions[i].Foldout)
			{
				EditorGUI.indentLevel++;
				actions[i].ActionGUI(attackPattern);
				//                    case(BulletAction.Type.Repeat):
				//                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
				//                        NestedBulletActionsGUI(ac);
				//                        break;
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}
		MoveRemoveAdd<IBulletAction, SharedAction.Wait>(moveRemove, actions);
		return actions.ToArray();
	}

	public static IFireAction[] FireActionGUI(IFireAction[] fireActions, AttackPattern attackPattern)
	{
		List<IFireAction> actions = new List<IFireAction>(fireActions);
		
		Vector3 moveRemove = new Vector3(-1f, -1f, -1f);
		for (int i = 0; i < actions.Count; i++)
		{
			Rect boundingRect = EditorGUILayout.BeginVertical();
			GUI.Box(boundingRect, "");
			EditorGUILayout.BeginHorizontal();
			actions[i].Foldout = EditorGUILayout.Foldout(actions[i].Foldout, actions[i].ToString());
			moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i);
			EditorGUILayout.EndHorizontal();
			if (actions[i].Foldout)
			{
				EditorGUI.indentLevel++;
				Type actionType = fireActionTypes[EditorGUILayout.Popup(fireActionTypes.Index(actions[i]),fireActionTypes.names)];
				if(actionType != actions[i].GetType())
				{
					actions[i] = (IFireAction)Activator.CreateInstance(actionType);
				}
				actions[i].ActionGUI(attackPattern);
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
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}
		MoveRemoveAdd<IFireAction, SharedAction.Wait>(moveRemove, actions);
		
		return actions.ToArray();
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

    private static void Swap<T>(List<T> list, int a, int b)
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


using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DanmakuPropertyAttribute))]
public class DanmakuPropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		SerializedProperty rank = property.FindPropertyRelative ("rank");
		return (rank.boolValue) ? 16 * (Enum.GetValues (typeof(Rank)).Length + 1) : 32f;
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		DanmakuPropertyAttribute dpa = attribute as DanmakuPropertyAttribute;
		SerializedProperty lower = property.FindPropertyRelative ("Lower");
		SerializedProperty upper = property.FindPropertyRelative ("Upper");
		SerializedProperty random = property.FindPropertyRelative ("random");
		SerializedProperty rank = property.FindPropertyRelative ("rank");
		SerializedProperty sequence = property.FindPropertyRelative ("sequence");
		Rect altRect = position;
		altRect.height = 16;
		altRect.width -= 90f;
		EditorGUI.LabelField (altRect, dpa.name);
		altRect.x += altRect.width;
		altRect.width = 30f;
		if(GUI.Button (altRect, (random.boolValue) ? '@'.ToString () : '\u2022'.ToString ()))
		{
			random.boolValue = !random.boolValue;
		}
		altRect.x += 30f;
		if(GUI.Button (altRect, (rank.boolValue) ? '\u2191'.ToString () + '\u2191'.ToString () : "--"))
		{
			rank.boolValue = !rank.boolValue;
		}
		altRect.x += 30f;
		GUI.enabled = dpa.useSequence;
		if(GUI.Button (altRect, (sequence.boolValue) ? ">>" : "--"))
		{
			sequence.boolValue = !sequence.boolValue;
		}
		GUI.enabled = true;
		altRect = position;
		altRect.height = 16;
		altRect.x += 20;
		altRect.width = (altRect.width - 30 / 2);
		altRect.y += altRect.height;
		float leftHandSide = altRect.x;
		int total = ((random.boolValue) ? 2 : 1);
		altRect.width /= total;
		float low, up;
		SerializedProperty lowI, upI;
		SerializedProperty lowS = lower.FindPropertyRelative("Array.size"), upS = upper.FindPropertyRelative("Array.size");
		string[] names = Enum.GetNames (typeof(Rank));
		while(names.Length > lowS.intValue)
		{
			lower.InsertArrayElementAtIndex (lowS.intValue);
		}
		while(names.Length > upS.intValue)
		{
			upper.InsertArrayElementAtIndex (upS.intValue);
		}
		if(rank.boolValue)
		{
			for(int i = 0; i < names.Length; i++)
			{
				lowI = lower.GetArrayElementAtIndex (i);
				upI = upper.GetArrayElementAtIndex (i);
				low = lowI.floatValue;
				up = upI.floatValue;
				altRect.x = leftHandSide;
				if(dpa.isInt)
				{
					low = EditorGUI.IntField(altRect, names[i], (int)low);
					if(random.boolValue)
					{
						altRect.x += altRect.width;
						up = EditorGUI.IntField(altRect, (int)up);
					}
				}
				else
				{
					low = EditorGUI.FloatField(altRect, names[i], low);
					if(random.boolValue)
					{
						altRect.x += altRect.width;
						up = EditorGUI.FloatField(altRect, up);
					}
				}
				altRect.y += altRect.height;
				lowI.floatValue = low;
				upI.floatValue = up;
			}
		}
		else
		{
			lowI = lower.GetArrayElementAtIndex (0);
			upI = upper.GetArrayElementAtIndex (0);
			low = lowI.floatValue;
			up = upI.floatValue;
			if(dpa.isInt)
			{
				low = EditorGUI.IntField(altRect, "Value", (int)low);
				if(random.boolValue)
				{
					altRect.x += altRect.width;
					up = EditorGUI.IntField(altRect, (int)up);
				}
			}
			else
			{
				low = EditorGUI.FloatField(altRect, "Value", low);
				if(random.boolValue)
				{
					altRect.x += altRect.width;
					up = EditorGUI.FloatField(altRect, up);
				}
			}
			for(int i = 0; i < lower.FindPropertyRelative("Array.size").intValue; i++)
			{
				lower.GetArrayElementAtIndex(i).floatValue = low;
			}
			for(int i = 0; i < upper.FindPropertyRelative("Array.size").intValue; i++)
			{
				upper.GetArrayElementAtIndex(i).floatValue = up;
			}
		}
	}
}


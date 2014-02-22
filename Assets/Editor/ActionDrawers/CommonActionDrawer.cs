using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Actions;

public class CommonActionDrawer
{
	public static ActionAttackPattern attackPattern;

	public static T EnumChoice<T>(SerializedProperty prop, string label)
	{
		int choice;
		if(prop.propertyType == SerializedPropertyType.Integer)
		{
			choice = prop.intValue;
		}
		else
		{
			choice = prop.enumValueIndex;
		}
		string[] names = Enum.GetNames (typeof(T));
		choice = EditorGUILayout.Popup (label, choice, names);
		if(prop.propertyType == SerializedPropertyType.Integer)
		{
			prop.intValue = choice;
		}
		else
		{
			prop.enumValueIndex = choice;
		}
		return ((T[])Enum.GetValues(typeof(T)))[choice];
	}

	public static void Fire(SerializedProperty prop, ActionAttackPattern attackPattern)
	{
		SerializedProperty source = prop.FindPropertyRelative ("source");
		SerializedProperty location = prop.FindPropertyRelative ("location");
		SerializedProperty randomStyle = prop.FindPropertyRelative ("randomStyle");
		SerializedProperty randomArea = prop.FindPropertyRelative ("randomArea");
		SerializedProperty sourceRadius = prop.FindPropertyRelative ("sourceRadius");
		SerializedProperty sourceTheta = prop.FindPropertyRelative ("sourceTheta");
		SerializedProperty direction = prop.FindPropertyRelative ("Direction");
		SerializedProperty obs = prop.FindPropertyRelative ("overwriteBulletSpeed");
		SerializedProperty speed = prop.FindPropertyRelative ("speed");
		SerializedProperty fake = prop.FindPropertyRelative ("fake");
		SerializedProperty bti = prop.FindPropertyRelative ("bulletTagIndex");
		SerializedProperty angle = prop.FindPropertyRelative ("angle");
		SerializedProperty mirror = prop.FindPropertyRelative ("mirrorFire");
		SerializedProperty soundClip = prop.FindPropertyRelative ("audioClip");

		SourceType st = EnumChoice<SourceType> (source, "Source");
		switch(st)
		{
			case SourceType.Attacker:
				break;
			case SourceType.Absolute:
			case SourceType.Relative:
			case SourceType.Sequence:
				EditorGUILayout.PropertyField(location);
				RandomStyle rs = EnumChoice<RandomStyle>(randomStyle, "Random Style");
				if(rs !=  RandomStyle.None)
				{
					EditorGUILayout.PropertyField(randomArea);
				}
				break;
		}
		EditorGUILayout.PropertyField (sourceRadius, new GUIContent ("Offset Radius"));
		EditorGUILayout.PropertyField (sourceTheta, new GUIContent ("Offset Theta"));

		EnumChoice<DirectionType> (direction, "Direction");
		EditorGUILayout.PropertyField (angle);
//		if (!action.useParam)
//		{
//			action.angle = AttackPattern.Property.EditorGUI("Angle", action.angle, false);
//		}
//		action.useParam = EditorGUILayout.Toggle("Use Param Angle", action.useParam);
		EditorGUILayout.Space();

		EditorGUILayout.PropertyField (obs);
		if(obs.boolValue)
		{
			EditorGUILayout.PropertyField(speed);
		}
		
//		EditorGUILayout.Space();
//		EditorGUILayout.BeginHorizontal();
//		action.passParam = EditorGUILayout.Toggle("PassParam", action.passParam);
//		if (!action.passParam)
//		{
//			action.passPassedParam = EditorGUILayout.Toggle("PassMyParam", action.passPassedParam);
//		}
//		EditorGUILayout.EndHorizontal();    
//		if (action.passParam)
//		{
//			action.paramRange = EditorGUILayout.Vector2Field("Param Range", action.paramRange);
//		}
		EditorGUILayout.PropertyField (fake);
		EditorGUILayout.PropertyField (mirror);
		EditorGUILayout.PropertyField (soundClip);
		DanmakuEditorUtils.NamedObjectPopup ("Bullet Tag", attackPattern.bulletTags, bti, "Bullet Tag");
	}
}


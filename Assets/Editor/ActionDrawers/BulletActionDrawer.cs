using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Core;

[CustomPropertyDrawer(typeof(Bullet.Action))]
public class BulletActionDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 0f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUILayout.LabelField ("MovementAction");
		SerializedProperty type = property.FindPropertyRelative ("type");
		SerializedProperty wait = property.FindPropertyRelative ("wait");
		SerializedProperty repeat = property.FindPropertyRelative ("repeat");
		SerializedProperty usingRepeat = property.FindPropertyRelative ("usingRepeat");
		SerializedProperty direction = property.FindPropertyRelative ("direction");
		SerializedProperty angle = property.FindPropertyRelative ("angle");
		SerializedProperty waitForFinish = property.FindPropertyRelative ("waitForFinish");
		SerializedProperty speed = property.FindPropertyRelative ("speed");
		SerializedProperty fake = property.FindPropertyRelative ("fake");
		SerializedProperty representation = property.FindPropertyRelative ("representation");
		Bullet.Action.Type bt = CommonActionDrawer.EnumChoice<Bullet.Action.Type> (type, "Type");
		representation.stringValue = bt.ToString ();
		usingRepeat.boolValue = bt == Bullet.Action.Type.Repeat;
		switch(bt)
		{
			case Bullet.Action.Type.Wait:
				EditorGUILayout.PropertyField(wait);
				break;
			case Bullet.Action.Type.Repeat:
				EditorGUILayout.PropertyField(repeat);
				break;
			case Bullet.Action.Type.ChangeDirection:
				EditorGUILayout.PropertyField(direction);
				EditorGUILayout.PropertyField(angle);
				EditorGUILayout.PropertyField(wait);
				EditorGUILayout.PropertyField(waitForFinish);
				break;
			case Bullet.Action.Type.ChangeSpeed:
				EditorGUILayout.PropertyField(speed);
				EditorGUILayout.PropertyField(wait);
				EditorGUILayout.PropertyField(waitForFinish);
				break;
			case Bullet.Action.Type.ChangeFake:
				EditorGUILayout.PropertyField(fake);
				break;
			case Bullet.Action.Type.Fire:
				CommonActionDrawer.Fire(property, CommonActionDrawer.attackPattern);
				break;
		}
	}
}


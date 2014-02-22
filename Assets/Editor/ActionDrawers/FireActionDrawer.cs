using System;
using UnityEngine;
using UnityEditor;
using DanmakuEngine.Actions;

[CustomPropertyDrawer(typeof(FireAction))]
public class FireaActionDrawer : PropertyDrawer
{
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		return 0f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUILayout.LabelField ("FireAction");
		SerializedProperty type = property.FindPropertyRelative ("type");
		SerializedProperty wait = property.FindPropertyRelative ("wait");
		SerializedProperty repeat = property.FindPropertyRelative ("repeat");
		SerializedProperty usingRepeat = property.FindPropertyRelative ("usingRepeat");
		SerializedProperty representation = property.FindPropertyRelative ("representation");
		SerializedProperty fti = property.FindPropertyRelative ("fireTagIndex");
		SerializedProperty wtf = property.FindPropertyRelative ("waitForFinish");
		SerializedProperty soundClip = property.FindPropertyRelative ("audioClip");
		FireAction.Type ft = CommonActionDrawer.EnumChoice<FireAction.Type> (type, "Type");
		representation.stringValue = ft.ToString ();
		usingRepeat.boolValue = ft == FireAction.Type.Repeat;
		switch(ft)
		{
			case FireAction.Type.Wait:
				EditorGUILayout.PropertyField (wait);
				break;
			case FireAction.Type.Repeat:
				EditorGUILayout.PropertyField(repeat);
				break;
			case FireAction.Type.CallFireTag:
				DanmakuEditorUtils.NamedObjectPopup("Fire Tag", CommonActionDrawer.attackPattern.fireTags, fti, "Fire Tag");
//				GUILayout.BeginHorizontal();
//				passParam = EditorGUILayout.Toggle("PassParam", passParam);
//				if(!passParam)
//					passPassedParam = EditorGUILayout.Toggle("PassMyParam", passPassedParam);
//				GUILayout.EndHorizontal();	
//				if(passParam)
//					paramRange = EditorGUILayout.Vector2Field("Param Range", paramRange);
				EditorGUILayout.PropertyField(wtf);
				break;
			case FireAction.Type.Fire:
				CommonActionDrawer.Fire(property, CommonActionDrawer.attackPattern);
				break;
			case FireAction.Type.PlaySoundEffect:
				EditorGUILayout.PropertyField(soundClip);
				break;
		}
	}
}


using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class FireAction : AttackPatternAction<FireAction, FireAction.Type>
{
	public enum Type { Wait, Repeat, CallFireTag, Fire }
	public override ActionType ActionType 
	{
		get
		{
			switch(type)
			{
				case Type.CallFireTag:
					return (waitForFinish) ? ActionType.Normal : ActionType.Coroutine;
				default:
					return ActionType.Normal;
			}
		}
	}

	public int fireTagIndex;

	#if UNITY_EDITOR
	public override void ActionGUI(params object[] param)
	{
		type = (Type)EditorGUILayout.EnumPopup("Type", type);
		AttackPattern attackPattern = param [0] as AttackPattern;
		switch(type)
		{
			case Type.Wait:
				wait = AttackPattern.Property.EditorGUI ("Wait", wait, false);
				break;
			case Type.Repeat:
				SharedAction.Repeat.ActionGUI<FireAction, FireAction.Type>(this, param);
				break;
			case Type.CallFireTag:
				fireTagIndex = EditorUtils.NamedObjectPopup("Fire Tag", attackPattern.fireTags, fireTagIndex, "Fire Tag");
				GUILayout.BeginHorizontal();
				passParam = EditorGUILayout.Toggle("PassParam", passParam);
				if(!passParam)
					passPassedParam = EditorGUILayout.Toggle("PassMyParam", passPassedParam);
				GUILayout.EndHorizontal();	
				if(passParam)
					paramRange = EditorGUILayout.Vector2Field("Param Range", paramRange);
				waitForFinish = EditorGUILayout.Toggle("Wait to Finish", waitForFinish);
				break;
			case Type.Fire:
				SharedAction.Fire.ActionGUI<FireAction, FireAction.Type>(this, attackPattern);
				break;
		}
	}

	public override void DrawGizmosImpl (FireAction previous)
	{

	}
	#endif

	public override IEnumerator Execute (params object[] param)
	{
		AttackPattern attackPattern = param [1] as AttackPattern;
		FireTag fireTag = param [0] as FireTag;
		switch(type)
		{
			case Type.Repeat:
				yield return parent.StartCoroutine(SharedAction.Repeat.Execute<FireAction, FireAction.Type>(nestedActions, repeat, param));
				break;
			case Type.CallFireTag:
				FireTag tag = attackPattern.fireTags[fireTagIndex];
				if(passParam)
				{
					tag.param = UnityEngine.Random.Range(paramRange.x, paramRange.y);
				}
				else if(passPassedParam)
				{
					tag.param = fireTag.param;
				}
				if(waitForFinish)
				{
					yield return parent.StartCoroutine(tag.Run());
				}
				else
				{
					parent.StartCoroutine(tag.Run(param));
				}
				break;
			case Type.Fire:
				SharedAction.Fire.Execute<FireAction, FireAction.Type>(this, parent as Enemy, attackPattern, 
			                                                       parent.transform.position, 
			                                                       parent.transform.rotation, 
			                                                       fireTag.param, 
			                                                       fireTag.previousRotation);
				break;
			case Type.Wait:
				yield return new WaitForSeconds(wait.Value);
				break;
		}
	}
}

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
//	public override ActionType ActionType 
//	{
//		get
//		{
//			switch(type)
//			{
//				case Type.CallFireTag:
//					return (waitForFinish) ? ActionType.Normal : ActionType.Coroutine;
//				default:
//					return ActionType.Normal;
//			}
//		}
//	}

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
		if(attackPattern.currentHealth < 0)
		{
			return false;
		}
		FireTag fireTag = param [0] as FireTag;
		Enemy master = parent as Enemy;
		switch(type)
		{
			case Type.Repeat:
				int repeatC = Mathf.FloorToInt(repeat.Value);
				for(int j = 0; j < repeatC; j++)
				{
					for(int i = 0; i < nestedActions.Length; i++)
					{
						if(attackPattern.currentHealth < 0)
						{
							return false;
						}
						else
						{
							yield return master.StartCoroutine(nestedActions[i].Execute(param[0], param[1]));	
						}
					}
				}
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
					yield return parent.StartCoroutine(tag.Run(param[1]));
				}
				else
				{
					parent.StartCoroutine(tag.Run(param[1]));
				}
				break;
			case Type.Fire:
				attackPattern.Fire<FireAction, FireAction.Type>(this, master, master.Transform.position, master.Transform.rotation, fireTag.param, fireTag.previousRotation);
				break;
			case Type.Wait:
				float totalTime = 0;
				float waitTime = wait.Value;
				while(totalTime < waitTime)
				{
					yield return parent.StartCoroutine(Global.WaitForUnpause());
					if(attackPattern.currentHealth < 0)
					{
						return false;
					}
					yield return new WaitForFixedUpdate();
					totalTime += Time.fixedDeltaTime;
				}
				break;
		}
	}
}

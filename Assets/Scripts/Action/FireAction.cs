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
		Enemy master = param[0] as Enemy;
		AttackPattern attackPattern = param [1] as AttackPattern;
		float fireTagParam = (float)param [2];
		Debug.Log (param.Length);
		RotationWrapper previousRotation = param [3] as RotationWrapper;
		IEnumerator pause, actionEnumerator;
		if(attackPattern.currentHealth < 0)
		{
			return false;
		}
		switch(type)
		{
			case Type.Repeat:
				int repeatC = Mathf.FloorToInt(repeat.Value);
				for(int j = 0; j < repeatC; j++)
				{
					foreach(Action action in nestedActions)
					{
						pause = Global.WaitForUnpause();
						while(pause.MoveNext())
						{
							yield return pause.Current;
						}
						if(attackPattern.currentHealth < 0)
						{
							return false;
						}
						else
						{
							actionEnumerator = action.Execute(master, attackPattern, fireTagParam, previousRotation);
							while(actionEnumerator.MoveNext())
							{
								yield return actionEnumerator.Current;
							}
						}
					}
				}
				break;
			case Type.CallFireTag:
				FireTag tag = attackPattern.fireTags[fireTagIndex];
				float floatParam = 0.0f;
				if(passParam)
				{
					floatParam = UnityEngine.Random.Range(paramRange.x, paramRange.y);
				}
				else if(passPassedParam)
				{
					floatParam = fireTagParam;
				}
				parent.StartCoroutine(tag.Run(param[1], floatParam));
				break;
			case Type.Fire:
				attackPattern.Fire<FireAction, FireAction.Type>(this, master, master.Transform.position, master.Transform.rotation, fireTagParam, previousRotation);
				break;
			case Type.Wait:
				float totalTime = 0;
				float waitTime = wait.Value;
				while(totalTime < waitTime)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
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

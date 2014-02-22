using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;

namespace DanmakuEngine.Actions
{
	
	[System.Serializable]
	public class FireAction : AttackPatternAction<FireAction, FireAction.Type>
	{
		public enum Type { Wait, Repeat, CallFireTag, Fire, PlaySoundEffect }
		public int fireTagIndex;
		
		public FireAction()
		{
			representation = type.ToString ();
		}

		protected override void DrawHandlesImpl (FireAction previous)
		{
		}
		
		public override IEnumerator Execute (params object[] param)
		{
			Debug.Log (param [0]);
			AbstractEnemy master = param[0] as AbstractEnemy;
			ActionAttackPattern attackPattern = param [1] as ActionAttackPattern;
			float fireTagParam = (float)param [2];
			SequenceWrapper sequence = param [3] as SequenceWrapper;
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
							actionEnumerator = action.Execute(master, attackPattern, fireTagParam, sequence);
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
				//				if(passParam)
				//				{
				//					floatParam = UnityEngine.Random.Range(paramRange.x, paramRange.y);
				//				}
				//				else if(passPassedParam)
				//				{
				//					floatParam = fireTagParam;
				//				}
				parent.StartCoroutine(tag.Run(master, attackPattern, floatParam));
				break;
			case Type.Fire:
				Debug.Log(master);
				Debug.Log(attackPattern);
				attackPattern.Fire<FireAction, FireAction.Type>(this, master, master.Transform.position, master.Transform.rotation, fireTagParam, sequence);
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
			case Type.PlaySoundEffect:
				SoundManager.PlaySoundEffect (audioClip, master.Transform.position);
				break;
			}
		}
	}

}
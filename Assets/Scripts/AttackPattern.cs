using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttackPattern : CachedObject, NamedObject
{
	public Enemy parent;
	public string bpName = "Attack Pattern";
	public int health;
	public int timeout;
	public int secondsRemaining;
	public int bonus;
	public bool survival;

	[NonSerialized]
	public int currentHealth;
	[NonSerialized]
	public int remainingBonus;

	public EnemyDrops drops;

	public string Name
	{
		get { return bpName; }
		set { bpName = value; }
	}

	public FireTag[] fireTags;
	public BulletTag[] bulletTags;
	public float sequenceSpeed = 0.0f;
	
	bool started = false;

	// Use this for initialization
	void Start () 
	{
		InitateFire();
	}

	void OnEnable()
	{
		InitateFire();
	}

	private IEnumerator InitateFire()
	{
		if(!started)
		{
			yield return new WaitForSeconds(1.0f);
		}
		started = true;

		while(true)
		{
			//yield return RunFire(fireTags[0], fireTags[0].actions, 1);
		}
	}

	private void RunFire(FireTag fireTag, IFireAction[] actions, int repeatC)
	{
		if(actions.Length == 0)
		{
			return;
		}
		else
		{
			for(int j = 0; j < repeatC; j++)
			{
				for(int i = 0; i < actions.Length; i++)
				{
					ActionExecutor.ExecuteAction(fireTag[i], fireTag);
//					FireAction currentAction = actions[i];
//					switch(currentAction.type)
//					{
//					case(FireAction.Type.Wait):
//						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
//						break;
//						
//					case(FireAction.Type.Fire):
//						Fire(currentAction.GetSourcePosition(Transform.position), Transform.rotation, currentAction, fireTag.param, fireTag.previousRotation);
//						break;
//						
//					case(FireAction.Type.CallFireTag	):
//						FireTag calledFireTag = fireTags[currentAction.fireTagIndex];
//						
//						if(currentAction.passParam)
//							calledFireTag.param = UnityEngine.Random.Range(currentAction.paramRange.x, currentAction.paramRange.y);
//						else if(currentAction.passPassedParam)
//							calledFireTag.param = fireTag.param;
//						
//						if(calledFireTag.actions.Length > 0)
//							yield return RunFire(calledFireTag, calledFireTag.actions, 1);
//						break;
//						
//					case(FireAction.Type.Repeat):
//						yield return RunFire(fireTag, currentAction.nestedActions, Mathf.FloorToInt(currentAction.repeat.Value));
//						break;
//					}
				}
			}
		}
	}

	public struct Property
	{
		private Vector3 values;
		public bool rank;
		public bool random;
		
		public float Value
		{
			get { return UnrankedValue + RankValue; }
		}
		
		public float UnrankedValue
		{
			get { return (random) ? RandomValue : FixedValue; }
		}
		
		public float FixedValue
		{
			get { return values.x; }
			set { values.x = value; }
		}
		
		public float RandomValue
		{
			get { return UnityEngine.Random.Range (values.x, values.y); }
		}
		
		public float RankValue
		{
			get { return (rank) ? (int)Global.Rank * values.z : 0; }
		}
		
		public float RankParam
		{
			get { return values.z; }
			set { values.z = value; }
		}
		
		public Vector2 RandomRange
		{
			get { return new Vector2(values.x, values.y); }
			set { values.x = value.x; values.y = value.y; }
		}

#if UNITY_EDITOR
		public void EditorGUI(string propName, bool isInt)
		{
			EditorGUILayout.BeginHorizontal();
			if (!random)
			{
				if(isInt)
				{
					FixedValue = (float)EditorGUILayout.IntField(propName, (int)FixedValue);
				}
				else
				{
					FixedValue = EditorGUILayout.FloatField(propName, FixedValue);
				}
			} 
			else
			{
				RandomRange = EditorGUILayout.Vector2Field(propName + " Range", RandomRange);
			}
			if (isInt)
			{
				random = false;
			}
			else
			{
				random = EditorGUILayout.Toggle("Randomize", random);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			rank = EditorGUILayout.Toggle("Add Rank", rank);
			if (rank)
			{
				RankParam = EditorGUILayout.FloatField("Rank Increase", RankParam);
			}
			EditorGUILayout.EndHorizontal();
		}
#endif
	}
}

public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence }

public enum SourceType { Attacker, Absolute, Relative, AnotherObject, ScreenEdge }

public class RotationWrapper
{
	public Quaternion rotation;
	public bool rotationNull = true;
}

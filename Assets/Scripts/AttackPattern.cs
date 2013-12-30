using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AttackPattern : AbstractActionBehavior<MovementAction, MovementAction.Type>, IActionGroup, NamedObject
{
	public override string ActionGUITitle { get { return "Movement Pattern"; } }
	public override object[] ActionParameters { get { return new object[] { Transform }; } }
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

	[Serializable]
	public class Property
	{
		public Vector3 values = Vector3.zero;
		public bool rank = false;
		public bool random = false;
		
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
		public static AttackPattern.Property EditorGUI(string propName, AttackPattern.Property prop, bool isInt)
		{
			AttackPattern.Property property = prop;

			if(property == null)
			{
				property = new AttackPattern.Property();
			}

			EditorGUILayout.BeginHorizontal();
			if (!property.random)
			{
				if(isInt)
				{
					property.FixedValue = (float)EditorGUILayout.IntField(propName, (int)property.FixedValue);
				}
				else
				{
					property.FixedValue = EditorGUILayout.FloatField(propName, property.FixedValue);
				}
			} 
			else
			{
				property.RandomRange = EditorGUILayout.Vector2Field(propName + " Range", property.RandomRange);
			}
			if (isInt)
			{
				property.random = false;
			}
			else
			{
				property.random = EditorGUILayout.Toggle("Randomize", property.random);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			property.rank = EditorGUILayout.Toggle("Add Rank", property.rank);
			if (property.rank)
			{
				property.RankParam = EditorGUILayout.FloatField("Rank Increase", property.RankParam);
			}
			EditorGUILayout.EndHorizontal();
			return property;
		}
		#endif
	}
}

public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence }
public enum SourceType { Attacker, Absolute, Relative, AnotherObject }
public enum RandomStyle { None, Rectangular, Elliptical }

[Serializable]
public abstract class AttackPatternAction<T, P> : NestedAction<T, P> where T : NestedAction<T, P> where P : struct, IConvertible
{	
	public AttackPattern.Property angle;
	public AttackPattern.Property speed;
	public DirectionType direction;
	public bool waitForFinish = false;
	
	public SourceType source;
	public RandomStyle randomStyle = RandomStyle.None;
	public Vector2 randomArea = Vector2.zero;
	public Vector2 location = Vector3.zero;
	public Transform alternateSource;

	public int bulletTagIndex;
	
	public BulletTag bulletTag;
	
	public bool useParam = false;
	public bool overwriteBulletSpeed = false;
	public bool useSequenceSpeed = false;
	
	public bool passParam = false;
	public bool passPassedParam = false;
	public Vector2 paramRange;
	
	public DirectionType Direction;
	
	public AudioClip audioClip = null;
}

public class RotationWrapper
{
	public Quaternion rotation;
	public bool rotationNull = true;
}

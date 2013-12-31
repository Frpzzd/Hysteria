using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AttackPattern : IActionGroup, NamedObject, TitledObject
{
	[SerializeField]
	public MonoBehaviour parent;
	[SerializeField]
	public string bpName;
	[SerializeField]
	public string title;
	[SerializeField]
	public int health = 100;
	[SerializeField]
	public int timeout;
	[SerializeField]
	public int secondsRemaining;
	[SerializeField]
	public int bonus;
	[SerializeField]
	public bool survival;

	[NonSerialized]
	public int currentHealth;
	[NonSerialized]
	public int remainingBonus;
	
	[SerializeField]
	public EnemyDrops drops;
	[SerializeField]
	public MovementAction[] actions;

	public AttackPattern()
	{
		fireTags = new FireTag[1];
		fireTags [0] = new FireTag ();
		bulletTags = new BulletTag[1];
		bulletTags [0] = new BulletTag ();
		actions = new MovementAction[1];
		actions [0] = new MovementAction ();
	}

	public string Name
	{
		get 
		{ 
			if(bpName == null)
			{
				bpName = "Attack Pattern";
			}
			return bpName; 
		}
		set { bpName = value; }
	}

	public string Title
	{
		get 
		{ 
			if(title == null)
			{
				title = "";
			}
			return title; 
		}
		set { title = value; }
	}

	[SerializeField]
	public FireTag[] fireTags;
	[SerializeField]
	public BulletTag[] bulletTags;

	[NonSerialized]
	public float sequenceSpeed = 0.0f;

	public IEnumerator Run(params object[] param)
	{
		parent.StartCoroutine (ActionHandler.ExecuteActions (actions, parent.transform));
		currentHealth = health;
		while(currentHealth > 0)
		{
			yield return parent.StartCoroutine(fireTags[0].Run(this));
		}
	}

	public void Initialize(MonoBehaviour parent)
	{
		this.parent = parent;
		foreach(MovementAction action in actions)
		{
			action.Initialize(parent);
		}
		foreach(FireTag tag in fireTags)
		{
			tag.Initialize(parent);
		}
		foreach(BulletTag tag in bulletTags)
		{
			tag.Initialize(parent);
		}
	}

	public void Damage(int amount)
	{
		currentHealth -= amount;
	}

	#if UNITY_EDITOR
	public void ActionGUI (params object[] param)
	{
		if(actions == null || actions.Length < 1)
		{
			actions = new MovementAction[1];
			actions[0] = new MovementAction();
		}

		EditorUtils.ExpandCollapseButtons<MovementAction, MovementAction.Type> ("Movement Pattern", actions);

		actions = EditorUtils.ActionGUI<MovementAction, MovementAction.Type> (actions, false, parent, param);
	}

	public void DrawGizmos(Color gizmoColor)
	{
	}
	#endif

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

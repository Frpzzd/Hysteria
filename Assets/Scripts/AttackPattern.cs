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
	[NonSerialized]
	public bool active;

	public IEnumerator Run(params object[] param)
	{
		active = true;
//		parent.StartCoroutine (Move ());
		currentHealth = health;
		while(currentHealth > 0)
		{
			yield return parent.StartCoroutine(fireTags[0].Run(this));
		}
		Debug.Log ("Exit Pattern");
		active = false;
	}

//	private IEnumerator Move()
//	{
//		Debug.Log ("Hello");
//		while(active)
//		{
//			for(int i = 0; i < actions.Length; i++)
//			{
//				yield return actions[i].parent.StartCoroutine(actions[i].Execute(parent.transform));
//			}
//		}
//	}

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
		Debug.Log (currentHealth);
	}

	public void Fire<T, P>(T action, Enemy master, Vector3 position, Quaternion rotation, float param, RotationWrapper previousRotation) 
		where T : AttackPatternAction<T, P>
			where P : struct, IConvertible
	{
		float angle,  speed;
		BulletTag bt = bulletTags[action.bulletTagIndex];
		Bullet temp = GameObjectManager.Bullets.Get(bt);
		if(previousRotation.rotationNull)
		{
			previousRotation.rotationNull = false;
			previousRotation.rotation = temp.Transform.localRotation;
		}
		
		temp.Transform.position = position;
		temp.Transform.rotation = rotation;
		
		if(action.useParam)
		{
			angle = param;
		}
		else
		{
			angle = action.angle.Value;
		}
		
		switch(action.Direction)
		{
		case (DirectionType.TargetPlayer):
			temp.Transform.LookAt(temp.Transform.position + Vector3.forward, Player.PlayerTransform.position - temp.Transform.position);
			break;
			
		case (DirectionType.Absolute):
			temp.Transform.localRotation = Quaternion.Euler(-(angle - 270), 270, 0);
			break;
			
		case (DirectionType.Relative):
			temp.Transform.localRotation = rotation * Quaternion.AngleAxis (-angle, Vector3.right);
			break;
			
		case (DirectionType.Sequence):
			temp.Transform.localRotation = previousRotation.rotation * Quaternion.AngleAxis (-angle, Vector3.right); 
			break;
		}
		previousRotation.rotation = temp.Transform.localRotation;
		if(action.overwriteBulletSpeed)
		{
			speed = action.speed.Value;
			
			if(action.useSequenceSpeed)
			{
				sequenceSpeed += speed;
				temp.velocity.x = sequenceSpeed;
			}
			else
			{
				sequenceSpeed = 0.0f;
				temp.velocity.x = speed;
			}
		}
		else
		{	
			temp.velocity.x = bt.speed.Value;
		}
		temp.velocity.y = 0f;
		
		if(action.passParam)
		{
			temp.param = UnityEngine.Random.Range(action.paramRange.x, action.paramRange.y);
		}
		
		if(action.passPassedParam)
		{
			temp.param = param;
		}
		temp.master = this;
		temp.GameObject.SetActive(true);
		SoundManager.PlaySoundEffect (action.audioClip, position);
	}
	
	public static Vector3 GetSourcePosition<T, P>(Vector3 currentPosition, T action) 
		where T : AttackPatternAction<T, P>
		where P : struct, IConvertible
	{
		switch(action.source)
		{
			case SourceType.Attacker:
				return currentPosition;
			case SourceType.Absolute:
				return GetRandomPosition(action.location, action.randomArea, currentPosition.z, action.randomStyle);
			case SourceType.Relative:
				return GetRandomPosition(new Vector2(currentPosition.x, currentPosition.y) + action.location, action.randomArea, currentPosition.z, action.randomStyle);
			case SourceType.AnotherObject:
				return action.alternateSource.position;
			default:
				return Vector3.zero;
		}
	}
	
	private static Vector3 GetRandomPosition(Vector2 start, Vector2 size, float z, RandomStyle randomStyle)
	{
		if(randomStyle == RandomStyle.Rectangular)
		{
			return new Vector3(start.x - 0.5f * size.x + size.x * UnityEngine.Random.value, start.y - 0.5f * size.y + size.y * UnityEngine.Random.value, z);
		}
		else if(randomStyle == RandomStyle.Elliptical)
		{
			float theta = 2 * Mathf.PI * UnityEngine.Random.value;
			float r = UnityEngine.Random.value;
			return new Vector3(start.x + size.x * r * Mathf.Cos(theta), start.y + size.y * r * Mathf.Sin(theta), z);
		}
		else
		{
			return new Vector3(start.x, start.y, z);
		}
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

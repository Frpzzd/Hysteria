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
	public Enemy parent;
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
	public List<Bullet> bulletsInPlay;

	[NonSerialized]
	public int currentHealth;
	[NonSerialized]
	public int remainingBonus;
	[NonSerialized]
	public float remainingTime;
	[NonSerialized]
	public bool success;
	
	[SerializeField]
	public EnemyDrops drops;
	[SerializeField]
	public MovementAction[] actions;

	//Editor stuff, remove on build
	public bool main;
	public FoldoutWrapper propertiesFoldout = new FoldoutWrapper();
	public FoldoutWrapper dropsFoldout = new FoldoutWrapper();
	public FoldoutWrapper movementFoldout = new FoldoutWrapper();
	public FoldoutWrapper fireTagFoldout = new FoldoutWrapper();
	public FoldoutWrapper bulletTagFoldout = new FoldoutWrapper();
	public SelectionType selectType = SelectionType.None;
	public int tagSelect;
	public Rank testRank;

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
		bulletsInPlay = new List<Bullet> ();
		currentHealth = health;
		parent.StartCoroutine (Move ());
		if(parent.boss)
		{
			parent.StartCoroutine (Timer ());
		}
		for(int i = 0; i < fireTags.Length; i++)
		{
			if(fireTags[i].runAtStart)
			{
				parent.StartCoroutine(fireTags[i].Run(param[0], this));
			}
		}
		while(currentHealth > 0)
		{
			yield return new WaitForFixedUpdate();
		}
		if(parent.boss)
		{
			foreach(Bullet bullet in bulletsInPlay.ToArray())
			{
				if(bullet.rend.isVisible)
				{
					bullet.Cancel();
				}
				else
				{
					bulletsInPlay.Remove(bullet);
				}
			}
		}
		else
		{
			bulletsInPlay.Clear();
		}
	}

	public void Stop()
	{
		Debug.Log ("Stop Pattern");
		currentHealth = -100;
	}

	private IEnumerator Move()
	{
		IEnumerator pause, actionEnumerator;
		foreach(Action action in actions)
		{
			pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			actionEnumerator = action.Execute(parent.transform, this);
			while(actionEnumerator.MoveNext())
			{
				yield return actionEnumerator.Current;
			}
		}
	}

	private IEnumerator Timer()
	{
		float deltat = Time.fixedDeltaTime;
		remainingTime = timeout;
		remainingBonus = bonus;
		success = true;
		IEnumerator pause;
		while(remainingTime > 0)
		{
			pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			if(currentHealth <= 0)
			{
				return false;
			}
			yield return new WaitForFixedUpdate();
			remainingTime -= deltat;
			remainingBonus = (success) ? (int)Mathf.Lerp((float)bonus, 0f, 1f - (remainingTime/timeout)) : 0;
		}
		Stop ();
	}

	public void Initialize(MonoBehaviour parent)
	{
		this.parent = (Enemy)parent;
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

	public void Fire<T, P>(T action, Enemy master, Vector3 position, Quaternion rotation, float param, RotationWrapper previousRotation) 
		where T : AttackPatternAction<T, P>
		where P : struct, IConvertible
	{
		if(!parent.renderer.isVisible)
		{
			return;
		}
		float angle,  speed;
		BulletTag bt = bulletTags[action.bulletTagIndex];
		Bullet temp = GameObjectManager.Bullets.Get(bt);
		if(previousRotation.rotationNull)
		{
			previousRotation.rotationNull = false;
			previousRotation.rotation = Quaternion.identity;
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
				Vector3 r = Player.PlayerTransform.position - temp.Transform.position;
				temp.Transform.localRotation = Quaternion.Euler(0, 0, angle + Mathf.Atan2(r.y, r.x) * (180f/Mathf.PI) - 90);
				break;
				
			case (DirectionType.Absolute):
				temp.Transform.localRotation = Quaternion.Euler(0, 0, angle);
				break;
				
			case (DirectionType.Relative):
				temp.Transform.localRotation = rotation * Quaternion.AngleAxis (-angle, Vector3.right);
				break;
				
			case (DirectionType.Sequence):
				temp.Transform.localRotation = Quaternion.Euler(previousRotation.rotation.eulerAngles + new Vector3(0, 0, angle)); 
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
		bulletsInPlay.Add (temp);
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

	public Vector3 DrawGizmos(Vector3 startPosition, Color gizmoColor)
	{
		Vector3 endLocation = startPosition;
		for(int i = 0; i < actions.Length; i++)
		{
			endLocation = actions[i].DrawGizmos(endLocation, gizmoColor);
		}
		return endLocation;
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

		public Vector2 EffectiveRange
		{
			get { return ((random) ? RandomRange : new Vector2 (FixedValue, FixedValue)) + new Vector2 (RankValue, RankValue); }
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
public enum SelectionType { None, Movement, Fire, Bullet }

public class FoldoutWrapper
{
	public bool Foldout;
}

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

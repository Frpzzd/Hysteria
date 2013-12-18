using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AttackPattern : MonoBehaviour, NamedObject
{
	[HideInInspector] 
	public GameObject BPgameObject;
	[HideInInspector]
	public Transform BPtransform;

	public bool bossPattern;
	public string bpName = "Attack Pattern";
	public int health;
	public int currentHealth;
	public int timeout;
	public int secondsRemaining;
	public int bonus;
	public int remainingBonus;
	public bool survival;
	public int bonusPerSecond;

	public string Name
	{
		get { return bpName; }
		set { bpName = value; }
	}

	public FireTag[] fireTags;
	public BulletTag[] bulletTags;
	public HashSet<Bullet> children;
	
	float sequenceSpeed = 0.0f;
	
	bool started = false;
	public float waitBeforeRepeating = 5.0f;

	void Awake()
	{
		BPgameObject = gameObject;
		BPtransform = transform;
		children = new HashSet<Bullet> ();
	}

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
			yield return RunFire(fireTags[0]);
		}
	}

	private IEnumerator RunFire(FireTag fireTag)
	{
		int index = 0;
		float deltat = Time.deltaTime;
		
		if(fireTag.actions.Length == 0)
		{
			Fire(BPtransform.position, BPtransform.rotation, fireTag.actions[index], fireTag.param, fireTag.previousRotation);
		}
		else
		{
			for(index = 0; index < fireTag.actions.Length; index++)
			{
				FireAction currentAction = fireTag.actions[index];
				switch(currentAction.type)
				{
					case(FireAction.Type.Wait):
						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
						break;
						
					case(FireAction.Type.Fire):
						Fire(currentAction.GetSourcePosition(BPtransform.position), BPtransform.rotation, currentAction, fireTag.param, fireTag.previousRotation);
						break;
						
					case(FireAction.Type.CallFireTag	):
						FireTag calledFireTag = fireTags[currentAction.fireTagIndex];
						
						if(currentAction.passParam)
							calledFireTag.param = UnityEngine.Random.Range(currentAction.paramRange.x, currentAction.paramRange.y);
						else if(currentAction.passPassedParam)
							calledFireTag.param = fireTag.param;
						
						if(calledFireTag.actions.Length > 0)
							yield return RunFire(calledFireTag);
						break;
						
					case(FireAction.Type.Repeat):
						yield return RunNestedFire(fireTag, currentAction);
						break;
				}
			}
		}
	}

	public IEnumerator RunNestedFire(FireTag ft, FireAction fa)
	{
		float deltat = Time.deltaTime;
		float repeatC = Mathf.Floor(fa.repeat.Value);

		for(int i = 0; i < repeatC; i++)
		{
			for(int j = 0; j < fa.nestedActions.Length; j++)
			{
				FireAction currentAction = fa.nestedActions[j];
				switch(currentAction.type)
				{
					case(FireAction.Type.Wait):
						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
						break;
						
					case(FireAction.Type.Fire):
						Fire(currentAction.GetSourcePosition(BPtransform.position), BPtransform.rotation, currentAction, ft.param, ft.previousRotation);
						break;
						
					case(FireAction.Type.CallFireTag	):
						FireTag calledFireTag = fireTags[currentAction.fireTagIndex];
						
						if(currentAction.passParam)
							calledFireTag.param = UnityEngine.Random.Range(currentAction.paramRange.x, currentAction.paramRange.y);
						else if(currentAction.passPassedParam)
							calledFireTag.param = ft.param;

						if(calledFireTag.actions.Length > 0)
							yield return RunFire(calledFireTag);
						break;
						
					case(FireAction.Type.Repeat):
						yield return RunNestedFire(ft, currentAction);
						break;
				}
			}
		}
	}

	public void Fire(Vector3 position, Quaternion rotation, Action action, float param, PreviousRotationWrapper previousRotation)
	{
		float angle, direction, angleDifference, speed;
		BulletTag bt = bulletTags[action.bulletTagIndex];
		Bullet temp = GameObjectManager.Bullets.Get(bt);
		if(previousRotation.prevRotationNull)
		{
			previousRotation.prevRotationNull = false;
			previousRotation.previousRotation = temp.trans.localRotation;
		}
		
		temp.trans.position = position;
		temp.trans.rotation = rotation;

		if(action.useParam)
		{
			angle = param;
		}
		else
		{
			angle = action.angle.Value;
		}

		switch(action.direction)
		{
			case (DirectionType.TargetPlayer):
				Quaternion originalRot = rotation;
				float dotHeading = Vector3.Dot( temp.trans.up, Player.playerTransform.position - temp.trans.position );
				
				if(dotHeading > 0)
				{
					direction = -1;
				}
				else
				{
					direction = 1;
				}
				angleDifference = Vector3.Angle(temp.trans.forward, Player.playerTransform.position - temp.trans.position);
				temp.trans.rotation = originalRot * Quaternion.AngleAxis((direction * angleDifference) - angle, Vector3.right);
				break;
				
			case (DirectionType.Absolute):
				temp.trans.localRotation = Quaternion.Euler(-(angle - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				temp.trans.localRotation = rotation * Quaternion.AngleAxis (-angle, Vector3.right);
				break;
				
			case (DirectionType.Sequence):
				temp.trans.localRotation = previousRotation.previousRotation * Quaternion.AngleAxis (-angle, Vector3.right); 
				break;
		}
		previousRotation.previousRotation = temp.trans.localRotation;
		if(action.overwriteBulletSpeed)
		{
			speed = action.speed.Value;
			
			if(action.useSequenceSpeed)
			{
				sequenceSpeed += speed;
				temp.speed = sequenceSpeed;
			}
			else
			{
				sequenceSpeed = 0.0f;
				temp.speed = speed;
			}
		}
		else
		{	
			temp.speed = bt.speed.Value;
		}
		temp.actions = bt.actions;
		
		if(action.passParam)
		{
			temp.param = UnityEngine.Random.Range(action.paramRange.x, action.paramRange.y);
		}

		if(action.passPassedParam)
		{
			temp.param = param;
		}
		temp.master = this;
		temp.gameObj.SetActive(true);
		if(action.audioClip != null)
		{
			AudioSource.PlayClipAtPoint(action.audioClip, position);
		}
	}
}

public enum DirectionType { TargetPlayer, Absolute, Relative, Sequence }

public enum SourceType { Attacker, Absolute, Relative, AnotherObject, ScreenEdge }

public interface NamedObject
{
	string Name { get; set; }
}

public class FireTag : Object, NamedObject
{
	private string ftName = "Fire Tag";
	public float param = 0.0f;
	public PreviousRotationWrapper previousRotation;
	public FireAction[] actions;

	public string Name
	{
		get
		{
			return ftName;
		}

		set
		{
			ftName = value;
		}
	}
}

public class BulletTag : Object, NamedObject
{
	private string btName = "Bullet Tag";
	public AttackPatternProperty speed;
	public Sprite sprite;
	public float colliderRadius;
	public Color colorMask;

	public BulletAction[] actions;

	public string Name
	{
		get
		{
			return btName;
		}
		
		set
		{
			btName = value;
		}
	}
}

public class PreviousRotationWrapper
{
	public Quaternion previousRotation;
	public bool prevRotationNull = true;
}

public abstract class Action
{
	public AttackPatternProperty wait;
	public AttackPatternProperty angle;
	public AttackPatternProperty speed;
	public AttackPatternProperty repeat;
	
	public DirectionType direction;
	public SourceType source;

	public bool randomSource = false;
	public Vector2 randomArea = Vector2.zero;
	public Vector3 location = Vector3.zero;
	public Transform alternateSource;

	public int bulletTagIndex;
	public bool useParam = false;
	public bool overwriteBulletSpeed = false;
	public bool useSequenceSpeed = false;

	public int fireTagIndex;
	public bool passParam = false;
	public bool passPassedParam = false;
	public Vector2 paramRange;

	public AudioClip audioClip = null;

	public Vector3 GetSourcePosition(Vector3 currentPos)
	{
		Vector3 value = Vector3.zero;
		switch(source)
		{
			case SourceType.Attacker:
				value = currentPos;
				break;
			case SourceType.AnotherObject:
				value = alternateSource.position;
				break;
			case SourceType.Absolute:
				value = location;
				break;
			case SourceType.Relative:
				value = new Vector3(currentPos.x + location.x, currentPos.y + location.y);
				break;
		}
		if(randomSource)
		{
			value.x += randomArea.x - (2 * randomArea.x * Random.value);
			value.y += randomArea.y - (2 * randomArea.y * Random.value);
		}
		return value;
	}
}

public abstract class NestedAction<T, P> : Action where T : NestedAction<T, P>
{
	public T[] nestedActions;
	public P type;

	#if UNITY_EDITOR
	public bool foldout = true;
	
	public void Expand(bool recursive)
	{
		SetAll (true, recursive);
	}
	
	public void Collapse(bool recursive)
	{
		SetAll (false, recursive);
	}
	
	private void SetAll(bool value, bool recursive)
	{
		foldout = value;
		if(recursive && nestedActions != null && nestedActions.Length > 0)
		{
			for(int i = 0; i < nestedActions.Length; i++)
			{
				nestedActions[i].SetAll(value, recursive);
			}
		}
	}
	#endif

}

public struct AttackPatternProperty
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
		get { return Random.Range (values.x, values.y); }
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
}

public class FireAction : NestedAction<FireAction, FireAction.Type>
{
	public enum Type { Wait, Fire, CallFireTag, Repeat }
}

public class BulletAction : NestedAction<BulletAction, BulletAction.Type>
{
	public enum Type { Wait, ChangeDirection, ChangeSpeed, Repeat, Fire, VerticalChangeSpeed, Deactivate }
	public bool waitForChange = false;
}

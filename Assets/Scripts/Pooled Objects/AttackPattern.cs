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
	
	float sequenceSpeed = 0.0f;
	
	bool started = false;
	public float waitBeforeRepeating = 5.0f;

	void Awake()
	{
		BPgameObject = gameObject;
		BPtransform = transform;
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
			Fire(BPtransform, fireTag.actions[index], fireTag.param, fireTag.previousRotation);
		}
		else
		{
			for(index = 0; index < fireTag.actions.Length; index++)
			{
				FireAction currentAction = fireTag.actions[index];
				switch(currentAction.type)
				{
					case(FireActionType.Wait):
						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
						break;
						
					case(FireActionType.Fire):
						Fire(BPtransform, currentAction, fireTag.param, fireTag.previousRotation);
						break;
						
					case(FireActionType.CallFireTag	):
						FireTag calledFireTag = currentAction.fireTag;
						
						if(currentAction.passParam)
							calledFireTag.param = UnityEngine.Random.Range(currentAction.paramRange.x, currentAction.paramRange.y);
						else if(currentAction.passPassedParam)
							calledFireTag.param = fireTag.param;
						
						if(calledFireTag.actions.Length > 0)
							yield return RunFire(calledFireTag);
						break;
						
					case(FireActionType.Repeat):
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
					case(FireActionType.Wait):
						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
						break;
						
					case(FireActionType.Fire):
						Fire(BPtransform, currentAction, ft.param, ft.previousRotation);
						break;
						
					case(FireActionType.CallFireTag	):
						FireTag calledFireTag = currentAction.fireTag;
						
						if(currentAction.passParam)
							calledFireTag.param = UnityEngine.Random.Range(currentAction.paramRange.x, currentAction.paramRange.y);
						else if(currentAction.passPassedParam)
							calledFireTag.param = ft.param;

						if(calledFireTag.actions.Length > 0)
							yield return RunFire(calledFireTag);
						break;
						
					case(FireActionType.Repeat):
						yield return RunNestedFire(ft, currentAction);
						break;
				}
			}
		}
	}

	public void Fire(Transform trans, BPAction action, float param, PreviousRotationWrapper previousRotation)
	{
		float angle, direction, angleDifference, speed;
		BulletTag bt = action.bulletTag;
		Bullet temp = GameObjectManager.Bullets.Get(bt);
		if(previousRotation.prevRotationNull)
		{
			previousRotation.prevRotationNull = false;
			previousRotation.previousRotation = temp.trans.localRotation;
		}
		temp.trans.position = trans.position;
		temp.trans.rotation = trans.rotation;
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
			case (DirectionType.Homing):
				Quaternion originalRot = trans.rotation;
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
				temp.trans.localRotation = trans.localRotation * Quaternion.AngleAxis (-angle, Vector3.right);
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
	}
}

public enum DirectionType { TargetPlayer, Homing, Absolute, Relative, Sequence }

public enum FireActionType { Wait, Fire, CallFireTag, Repeat }

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

public abstract class BPAction
{
	public AttackPatternProperty wait;
	public AttackPatternProperty angle;
	public AttackPatternProperty speed;
	public AttackPatternProperty repeat;
	
	public DirectionType direction;

	public BulletTag bulletTag;
	public bool useParam = false;
	public bool overwriteBulletSpeed = false;
	public bool useSequenceSpeed = false;

	public FireTag fireTag;
	public bool passParam = false;
	public bool passPassedParam = false;
	public Vector2 paramRange;
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

public class FireAction : BPAction
{
	public AudioClip audioClip = null;
	public FireActionType type = FireActionType.Wait; 
	public FireAction[] nestedActions;
}

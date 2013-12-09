using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AttackPattern : MonoBehaviour
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
		float waitT;
		
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
						if(currentAction.randomWait)
						{
							waitT = UnityEngine.Random.Range(currentAction.waitTime.x, currentAction.waitTime.y);
						}
						else
						{
							waitT = currentAction.waitTime.x;
						}
						if(currentAction.rankWait)
						{
							waitT += (int)Global.Rank * currentAction.waitTime.z;
						}
						waitT *= Time.deltaTime;
						yield return new WaitForSeconds(waitT);
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
		float waitT;
		
		float repeatC = fa.repeatCount.x;
		if(fa.rankRepeat)
		{
			repeatC += fa.repeatCount.y * (int)Global.Rank;
		}
		repeatC = Mathf.Floor(repeatC);

		for(int i = 0; i < repeatC; i++)
		{
			for(int j = 0; j < fa.nestedActions.Length; j++)
			{
				FireAction currentAction = fa.nestedActions[j];
				switch(currentAction.type)
				{
					case(FireActionType.Wait):
						if(currentAction.randomWait)
							waitT = UnityEngine.Random.Range(currentAction.waitTime.x, currentAction.waitTime.y);
						else
							waitT = currentAction.waitTime.x;
						if(currentAction.rankWait)
							waitT += (int)Global.Rank * currentAction.waitTime.z;
						waitT *= Time.deltaTime;
						yield return new WaitForSeconds(waitT);
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
		Bullet temp = GameObjectManager.Bullets.Get(Bullet.SpawnParams());
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
			if(action.randomAngle)
			{
				angle = UnityEngine.Random.Range(action.angle.x, action.angle.y);
			}
			else
			{
				angle = action.angle.x;
			}
			if(action.rankAngle)
			{
				angle += (int)Global.Rank * action.angle.z;
			}
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
			if(action.randomSpeed)
			{
				speed = UnityEngine.Random.Range(action.speed.x, action.speed.y);
			}
			else
			{
				speed = action.speed.x;	
			}
			if(action.rankSpeed)
			{
				speed += (int)Global.Rank * action.speed.z;
			}
			
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
			if(bt.randomSpeed)
			{
				temp.speed = UnityEngine.Random.Range(bt.speed.x, bt.speed.y);
			}
			else
			{
				temp.speed = bt.speed.x;
			}
			if(bt.rankSpeed)
			{
				temp.speed += (int)Global.Rank * bt.speed.z;
			}
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

[Serializable]
public enum DirectionType { TargetPlayer, Homing, Absolute, Relative, Sequence }

[Serializable]
public enum FireActionType { Wait, Fire, CallFireTag, Repeat }

public abstract class Tag : UnityEngine.Object
{
	public abstract string tagName { get; set; }
}

[Serializable]
public class FireTag : Tag
{
	private string ftName = "Fire Tag";
	public float param = 0.0f;
	public PreviousRotationWrapper previousRotation;
	public FireAction[] actions;

	public override string tagName
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

[Serializable]
public class BulletTag : Tag
{
	private string btName = "Bullet Tag";
	public Vector3 speed;
	public bool randomSpeed = false;
	public bool rankSpeed = false;
	public GameObject prefab = null;
	public BulletAction[] actions;

	public override string tagName
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

[Serializable]
public class PreviousRotationWrapper
{
	public Quaternion previousRotation;
	public bool prevRotationNull = true;
}

[Serializable]
public abstract class BPAction
{
	public Vector3 waitTime;
	public bool randomWait = false;
	public bool rankWait = false;
	
	public DirectionType direction;
	public Vector3 angle;
	public bool randomAngle = false;
	public bool rankAngle = false;
	
	public bool overwriteBulletSpeed = false;
	public Vector3 speed;
	public bool randomSpeed = false;
	public bool rankSpeed = false;
	public bool useSequenceSpeed = false;

	public BulletTag bulletTag;
	public bool useParam = false;

	public FireTag fireTag;
	public Vector2 repeatCount;
	public bool rankRepeat = false;
	
	public bool passParam = false;
	public bool passPassedParam = false;
	public Vector2 paramRange;
}

[Serializable]
public class FireAction : BPAction
{
	public AudioClip audioClip = null;
	public FireActionType type = FireActionType.Wait; 
	public FireAction[] nestedActions;
}

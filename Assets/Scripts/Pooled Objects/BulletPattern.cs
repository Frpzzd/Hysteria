using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BulletPattern : MonoBehaviour 
{
	
	[HideInInspector] 
	public GameObject BPgameObject;
	[HideInInspector]
	public Transform BPtransform;

	public bool bossPattern;
	public string bpName;
	public int maxHealth;
	public int currentHealth;
	public int timeOut;
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

	public bool ftFoldout = false;
	public List<bool> ftFoldouts = new List<bool>();
	public bool btFoldout = false;
	public List<bool> btFoldouts = new List<bool>();
	public List<ActionFoldouts> ftaFoldouts = new List<ActionFoldouts>();
	public List<ActionFoldouts> btaFoldouts = new List<ActionFoldouts>();

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
			yield return RunFire(0);
		}
	}

	private IEnumerator RunFire(int i)
	{
		FireTag fireTag = fireTags[i];
		IndexWrapper iw = new IndexWrapper();
		float waitT;
		int index;
		
		if(fireTag.actions.Length == 0)
		{
			Fire(BPtransform, fireTag.actions[iw.index], fireTag.param, fireTag.previousRotation);
		}
		else
		{
			for(iw.index = 0;iw.index < fireTag.actions.Length; iw.index++)
			{
				switch(fireTag.actions[iw.index].type)
				{
					case(FireActionType.Wait):
						if(fireTag.actions[iw.index].randomWait)
						{
							waitT = Random.Range(fireTag.actions[iw.index].waitTime.x, fireTag.actions[iw.index].waitTime.y);
						}
						else
						{
							waitT = fireTag.actions[iw.index].waitTime.x;
						}
						if(fireTag.actions[iw.index].rankWait)
						{
							waitT += Global.Rank * fireTag.actions[iw.index].waitTime.z;
						}
						waitT *= Global.TimePerFrame;
						yield return new WaitForSeconds(waitT);
						break;
						
					case(FireActionType.Fire):
						Fire(BPtransform, fireTag.actions[iw.index], fireTag.param, fireTag.previousRotation);
						break;
						
					case(FireActionType.CallFireTag	):
						index = fireTag.actions[iw.index].fireTagIndex - 1;
						
						if(fireTag.actions[iw.index].passParam)
							fireTags[index].param = Random.Range(fireTag.actions[iw.index].paramRange.x, fireTag.actions[iw.index].paramRange.y);
						else if(fireTag.actions[iw.index].passPassedParam)
							fireTags[index].param = fireTag.param;
						
						if(fireTags[index].actions.Length > 0)
							yield return RunFire(index);
						break;
						
					case(FireActionType.StartRepeat	):
						yield return RunNestedFire(i, iw);
						break;
				}
			}
		}
	}

	public IEnumerator RunNestedFire(int i, IndexWrapper iw)
	{
		FireTag fireTag = fireTags[i];
		int startIndex = iw.index;
		int endIndex = 0;
		int index;
		float waitT;
		
		float repeatC = fireTag.actions[iw.index].repeatCount.x;
		if(fireTag.actions[iw.index].rankRepeat)
			repeatC += fireTag.actions[iw.index].repeatCount.y * Global.Rank;
		repeatC = Mathf.Floor(repeatC);
		
		iw.index++;
		
		for(int y = 0; y < repeatC; y++)
		{
			while(fireTag.actions[iw.index].type != FireActionType.EndRepeat)
			{
				switch(fireTag.actions[iw.index].type)
				{
				case(FireActionType.Wait):
					if(fireTag.actions[iw.index].randomWait)
						waitT = Random.Range(fireTag.actions[iw.index].waitTime.x, fireTag.actions[iw.index].waitTime.y);
					else
						waitT = fireTag.actions[iw.index].waitTime.x;
					if(fireTag.actions[iw.index].rankWait)
						waitT += Global.Rank * fireTag.actions[iw.index].waitTime.z;
					waitT *= Global.TimePerFrame;
					yield return new WaitForSeconds(waitT);
					break;
					
				case(FireActionType.Fire):
					Fire(BPtransform, fireTag.actions[iw.index], fireTag.param, fireTag.previousRotation);
					break;
					
				case(FireActionType.CallFireTag	):
					index = fireTag.actions[iw.index].fireTagIndex - 1;
					
					if(fireTag.actions[iw.index].passParam)
						fireTags[index].param = Random.Range(fireTag.actions[iw.index].paramRange.x, fireTag.actions[iw.index].paramRange.y);
					else if(fireTag.actions[iw.index].passPassedParam)
						fireTags[index].param = fireTag.param;

					if(fireTags[index].actions.Length > 0)
						yield return RunFire(index);
					break;
					
				case(FireActionType.StartRepeat	):
					yield return RunNestedFire(i, iw);
					break;
				}
				
				iw.index++;
				
			}
			
			endIndex = iw.index;
			iw.index = startIndex+1;
		}
		
		iw.index = endIndex;

	}

	public void Fire(Transform trans, BPAction action, float param, PreviousRotationWrapper previousRotation)
	{
		float angle, direction, angleDifference, speed;
		BulletTag bt = bulletTags[action.bulletTagIndex - 1];
		Bullet temp = GameObjectManager.Bullets.Get();
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
				angle = Random.Range(action.angle.x, action.angle.y);
			}
			else
			{
				angle = action.angle.x;
			}
			if(action.rankAngle)
			{
				angle += Global.Rank * action.angle.z;
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
				speed = Random.Range(action.speed.x, action.speed.y);
			}
			else
			{
				speed = action.speed.x;	
			}
			if(action.rankSpeed)
			{
				speed += Global.Rank * action.speed.z;
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
				temp.speed = Random.Range(bt.speed.x, bt.speed.y);
			}
			else
			{
				temp.speed = bt.speed.x;
			}
			if(bt.rankSpeed)
			{
				temp.speed += Global.Rank * bt.speed.z;
			}
		}
		temp.actions = bt.actions;
		
		if(action.passParam)
		{
			temp.param = Random.Range(action.paramRange.x, action.paramRange.y);
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

public enum FireActionType { Wait, Fire, CallFireTag, StartRepeat, EndRepeat }

public class IndexWrapper
{
	public int index;
}

public class FireTag
{
	public float param = 0.0f;
	public PreviousRotationWrapper previousRotation;
	public FireAction[] actions;
}

public class BulletTag
{
	public Vector3 speed;
	public bool randomSpeed = false;
	public bool rankSpeed = false;
	public GameObject prefab = null;
	public BulletAction[] actions;
}

public class PreviousRotationWrapper
{
	public Quaternion previousRotation;
	public bool prevRotationNull = true;
}

public class BPAction
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
	
	public int bulletTagIndex = 0;
	public bool useParam = false;
	
	public int fireTagIndex = 0;
	public Vector2 repeatCount;
	public bool rankRepeat = false;
	
	public bool passParam = false;
	public bool passPassedParam = false;
	public Vector2 paramRange;
}

public class FireAction : BPAction
{
	public AudioClip audioClip = null;
	public FireActionType type = FireActionType.Wait; 
}

public class ActionFoldouts
{
	public bool main = false;
	public List<bool> sub = new List<bool>();
}

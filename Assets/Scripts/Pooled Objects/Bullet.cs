using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : PooledGameObject<BulletSpawmParams>
{
	[HideInInspector]
	public AttackPattern master;
	[HideInInspector]
	public BulletAction[] actions;
	[HideInInspector]
	public PreviousRotationWrapper prevRotation= new PreviousRotationWrapper();
	
	[HideInInspector]
	public float speed = 5.0f;
	[HideInInspector]
	public float verticalSpeed = 0.0f;
	[HideInInspector]
	public bool useVertical = false;
	[HideInInspector]
	public bool grazed;
	[HideInInspector]
	public float param = 0.0f;
		
	private CircleCollider2D col;
	private SpriteRenderer rend;

	public override void Awake ()
	{
		base.Awake ();
		col = (CircleCollider2D)collider2D;
		rend = (SpriteRenderer)renderer;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		Vector3 velocity = trans.forward * speed;
		if(useVertical)
		{
			velocity += (Vector3.up * verticalSpeed);
		}
		trans.position += velocity;
	}

	public override void Activate (BulletSpawmParams param)
	{
		grazed = false;
		rend.sprite = param.sp;
		rend.color = param.colorMask;
		col.radius = param.colliderRadius;
		RunActions();
	}

	public void Deactivate()
	{
		if(gameObj.activeSelf)
		{
			GameObjectManager.Bullets.Return(this);
		}
	}

	public IEnumerator RunActions()
	{	
		float waitT;
		for(int i = 0; i < actions.Length; i++)
		{
			switch(actions[i].type)
			{
				case(BulletActionType.Wait):			
					if(actions[i].randomWait)
					{
						waitT = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
					}
					else
					{
						waitT = actions[i].waitTime.x;
					}
					if(actions[i].rankWait)
					{
						waitT += (int)Global.Rank * actions[i].waitTime.z;
					}
					waitT *= Time.deltaTime;
					yield return new WaitForSeconds(waitT);
					break;
				case(BulletActionType.ChangeDirection):
					if(actions[i].waitForChange)
					{
						yield return ChangeDirection(actions[i]);
					}
					else
					{
						ChangeDirection(actions[i]);
					}
					break;
				case(BulletActionType.ChangeSpeed):
					if(actions[i].waitForChange)
					{
						yield return ChangeSpeed(actions[i], false);
					}
					else
					{
						ChangeSpeed(actions[i], false);
					}
					break;
				case(BulletActionType.Repeat):
					yield return RunNestedActions(actions[i]);
					break;
				case(BulletActionType.Fire):
					if(master != null)
					{
						master.Fire(trans, actions[i], param, prevRotation);
					}
					break;
				case(BulletActionType.VerticalChangeSpeed):
					if(actions[i].waitForChange)
					{
						yield return ChangeSpeed(actions[i], true);
					}
					else
					{
						ChangeSpeed(actions[i], true);
					}
					break;
				case(BulletActionType.Deactivate):
					Deactivate();
					break;
			}
		}
	}

	public IEnumerator RunNestedActions(BulletAction ba)
	{
		float waitT;
		
		float repeatC = ba.repeatCount.x;
		if(ba.rankRepeat)
		{
			repeatC += ba.repeatCount.y * (int)Global.Rank;
		}
		repeatC = Mathf.Floor(repeatC);

		for(int i = 0; i < repeatC; i++)
		{
			for(int j = 0; j < ba.nestedActions.Length; j++)
			{
				BulletAction currentAction = ba.nestedActions[j];
				switch(currentAction.type)
				{
					case(BulletActionType.Wait):
						if(currentAction.randomWait)
						{
							waitT = Random.Range(currentAction.waitTime.x, currentAction.waitTime.y);
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
					case(BulletActionType.ChangeDirection):
						if(currentAction.waitForChange)
						{
							yield return ChangeDirection(actions[i]);
						}
						else
						{
							ChangeDirection(actions[i]);
						}
						break;
					case(BulletActionType.ChangeSpeed):
						if(currentAction.waitForChange)
						{
							yield return ChangeSpeed(actions[i], false);
						}
						else
						{
							ChangeSpeed(actions[i], false);
						}
						break;
					case(BulletActionType.Repeat):
						yield return RunNestedActions(actions[i]);
						break;
					case(BulletActionType.Fire):
						if(master != null)
						{
							master.Fire(trans, currentAction, param, prevRotation);
						}
						break;
					case(BulletActionType.VerticalChangeSpeed):
						if(currentAction.waitForChange)
						{
							yield return ChangeSpeed(currentAction, true);
						}
						else
						{
							ChangeSpeed(currentAction, true);
						}
						break;
					case(BulletActionType.Deactivate):
						Deactivate();
						break;
				}
			}
		}
	}

	public IEnumerator ChangeDirection(BulletAction ba)
	{
		float t = 0.0f, d, ang;
		int dir;
		Quaternion newRot = Quaternion.identity;

		if(ba.randomWait)
			d = Random.Range(ba.waitTime.x, ba.waitTime.y);
		else
			d = ba.waitTime.x;
		if(ba.rankWait)
			d += (int)Global.Rank * ba.waitTime.z;
		
		d *= Time.deltaTime;
		
		Quaternion originalRot = trans.localRotation;
		
		// determine offset
		if(ba.randomAngle)
			ang = Random.Range(ba.angle.x, ba.angle.y);
		else
			ang = ba.angle.x;
		if(ba.rankAngle)
			ang += (int)Global.Rank * ba.angle.z;
		
		//and set rotation depending on angle
		switch(ba.direction)
		{
		case (DirectionType.TargetPlayer):
			float dotHeading = Vector3.Dot( trans.up, Player.playerTransform.position - trans.position );		
			if(dotHeading > 0)
				dir = -1;
			else
				dir = 1;
			float angleDif = Vector3.Angle(trans.forward, Player.playerTransform.position - trans.position);
			newRot = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right); 
			break;
			
		case (DirectionType.Absolute):
			newRot = Quaternion.Euler(-(ang - 270), 270, 0);
			break;
			
		case (DirectionType.Relative):
			newRot = originalRot * Quaternion.AngleAxis(-ang, Vector3.right);
			break;
			
		}
		
		//Sequence has its own thing going on, continually turning a set amount until time is up
		if(ba.direction == DirectionType.Sequence)
		{
			newRot = Quaternion.AngleAxis (-ang, Vector3.right); 
			
			while(t < d)
			{
				trans.localRotation *= newRot;
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
		//all the others just linearly progress to destination rotation
		else if(d > 0)
		{
			while(t < d)
			{
				trans.localRotation = Quaternion.Slerp(originalRot, newRot, t/d);
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			trans.localRotation = newRot;
		}
	}

	//its basically the same as the above except without rotations
	public IEnumerator ChangeSpeed(BulletAction ba, bool isVertical)
	{
		float t = 0.0f;
		float s = 0.0f;
		float d, newSpeed;
		
		if(isVertical)
			useVertical = true;
		
		if(ba.randomWait)
			d = Random.Range(ba.waitTime.x, ba.waitTime.y);
		else
			d = ba.waitTime.x;
		if(ba.rankWait)
			d += (int)Global.Rank * ba.waitTime.z;
		d *= Time.deltaTime;	
		
		float originalSpeed = speed;
		
		if(ba.randomSpeed)
			newSpeed = Random.Range(ba.speed.x, ba.speed.y);
		else
			newSpeed = ba.speed.x;
		if(ba.rankSpeed)
			d += (int)Global.Rank * ba.speed.z;
		
		if(d > 0)
		{
			while(t < d)
			{
				s = Mathf.Lerp(originalSpeed, newSpeed, t/d);
				if(isVertical) verticalSpeed = s;
				else speed = s;
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
		
		if(isVertical) verticalSpeed = newSpeed;
		else speed = newSpeed;
	}

	public void Cancel()
	{
		//Spawn Point Value at current location
		Deactivate();
	}

	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.CompareTag("Graze Hitbox") && !grazed)
		{
			Global.Graze++;
			grazed = true;
		}
	}

	public static BulletSpawmParams SpawnParams(Sprite sp, Color colorMask, float colliderRadius)
	{
		BulletSpawmParams bps = new BulletSpawmParams ();
		bps.colliderRadius = colliderRadius;
		bps.sp = sp;
		bps.colorMask = colorMask;
		return bps;
	}
}

public class BulletSpawmParams
{
	public Sprite sp;
	public Color colorMask;
	public float colliderRadius;
}

public class BulletAction : BPAction
{
	public BulletActionType type = BulletActionType.Wait;
	public bool waitForChange = false;
	public BulletAction[] nestedActions;
}

public enum BulletActionType { Wait, ChangeDirection, ChangeSpeed, Repeat, Fire, VerticalChangeSpeed, Deactivate }
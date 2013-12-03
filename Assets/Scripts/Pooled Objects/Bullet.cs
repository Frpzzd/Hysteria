using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : PooledGameObject<BulletSpawmParams>
{
	[HideInInspector]
	public BulletPattern master;
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
	[HideInInspector]
	public int actionIndex = 0;
	
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
		for(actionIndex = 0; actionIndex < actions.Length; actionIndex++)
		{
			switch(actions[actionIndex].type)
			{
				case(BulletActionType.Wait):			
					if(actions[actionIndex].randomWait)
					{
						waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
					}
					else
					{
						waitT = actions[actionIndex].waitTime.x;
					}
					if(actions[actionIndex].rankWait)
					{
						waitT += Global.Rank * actions[actionIndex].waitTime.z;
					}
					waitT *= Time.deltaTime;
					yield return new WaitForSeconds(waitT);
					break;
				case(BulletActionType.Change_Direction):
					if(actions[actionIndex].waitForChange)
					{
						yield return ChangeDirection(actionIndex);
					}
					else
					{
						ChangeDirection(actionIndex);
					}
					break;
				case(BulletActionType.Change_Speed):
					if(actions[actionIndex].waitForChange)
					{
						yield return ChangeSpeed(actionIndex, false);
					}
					else
					{
						ChangeSpeed(actionIndex, false);
					}
					break;
				case(BulletActionType.Start_Repeat):
					yield return RunNestedActions();
					break;
				case(BulletActionType.Fire):
					if(master != null)
					{
						master.Fire(trans, actions[actionIndex], param, prevRotation);
					}
					break;
				case(BulletActionType.Vertical_Change_Speed):
					if(actions[actionIndex].waitForChange)
					{
						yield return ChangeSpeed(actionIndex, true);
					}
					else
					{
						ChangeSpeed(actionIndex, true);
					}
					break;
				case(BulletActionType.Deactivate):
					Deactivate();
					break;
			}
		}
	}

	public IEnumerator RunNestedActions()
	{
		int startIndex = actionIndex;
		int endIndex = 0;
		actionIndex++;
		float waitT;
		
		float repeatC = actions[startIndex].repeatCount.x;
		if(actions[startIndex].rankRepeat)
		{
			repeatC += actions[startIndex].repeatCount.y * Global.Rank;
		}
		repeatC = Mathf.Floor(repeatC);

		for(int y = 0; y < repeatC; y++)
		{
			while(actions[actionIndex].type != BulletActionType.End_Repeat)
			{
				switch(actions[actionIndex].type)
				{
					case(BulletActionType.Wait):
						if(actions[actionIndex].randomWait)
						{
							waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
						}
						else
						{
							waitT = actions[actionIndex].waitTime.x;
						}
						if(actions[actionIndex].rankWait)
						{
							waitT += Global.Rank * actions[actionIndex].waitTime.z;
						}
						waitT *= Time.deltaTime;
						yield return new WaitForSeconds(waitT);
						break;
					case(BulletActionType.Change_Direction):
						if(actions[actionIndex].waitForChange)
						{
							yield return ChangeDirection(actionIndex);
						}
						else
						{
							ChangeDirection(actionIndex);
						}
						break;
					case(BulletActionType.Change_Speed):
						if(actions[actionIndex].waitForChange)
						{
							yield return ChangeSpeed(actionIndex, false);
						}
						else
						{
							ChangeSpeed(actionIndex, false);
						}
						break;
					case(BulletActionType.Start_Repeat):
						yield return RunNestedActions();
						break;
					case(BulletActionType.Fire):
						if(master != null)
						{
							master.Fire(trans, actions[actionIndex], param, prevRotation);
						}
						break;
					case(BulletActionType.Vertical_Change_Speed):
						if(actions[actionIndex].waitForChange)
						{
							yield return ChangeSpeed(actionIndex, true);
						}
						else
						{
							ChangeSpeed(actionIndex, true);
						}
						break;
					case(BulletActionType.Deactivate):
						Deactivate();
						break;
				}
				actionIndex++;
			}
			endIndex = actionIndex;
			actionIndex = startIndex+1;
		}
		actionIndex = endIndex;
	}

	public IEnumerator ChangeDirection(int i)
	{
		float t = 0.0f, d, ang;
		int dir;
		Quaternion newRot = Quaternion.identity;

		if(actions[i].randomWait)
			d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
		else
			d = actions[i].waitTime.x;
		if(actions[i].rankWait)
			d += Global.Rank * actions[i].waitTime.z;
		
		d *= Time.deltaTime;
		
		Quaternion originalRot = trans.localRotation;
		
		// determine offset
		if(actions[i].randomAngle)
			ang = Random.Range(actions[i].angle.x, actions[i].angle.y);
		else
			ang = actions[i].angle.x;
		if(actions[i].rankAngle)
			ang += Global.Rank * actions[i].angle.z;
		
		//and set rotation depending on angle
		switch(actions[i].direction)
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
		if(actions[i].direction == DirectionType.Sequence)
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
	public IEnumerator ChangeSpeed(int i, bool isVertical)
	{
		float t = 0.0f;
		float s = 0.0f;
		float d, newSpeed;
		
		if(isVertical)
			useVertical = true;
		
		if(actions[i].randomWait)
			d = Random.Range(actions[i].waitTime.x, actions[i].waitTime.y);
		else
			d = actions[i].waitTime.x;
		if(actions[i].rankWait)
			d += Global.Rank * actions[i].waitTime.z;
		d *= Time.deltaTime;	
		
		float originalSpeed = speed;
		
		if(actions[i].randomSpeed)
			newSpeed = Random.Range(actions[i].speed.x, actions[i].speed.y);
		else
			newSpeed = actions[i].speed.x;
		if(actions[i].rankSpeed)
			d += Global.Rank * actions[i].speed.z;
		
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

	public static BulletSpawmParams SpawnParams()
	{
		return null;
	}
}

public class BulletSpawmParams
{
}

public class BulletAction : BPAction
{
	public BulletActionType type = BulletActionType.Wait;
	public bool waitForChange = false;
}

public enum BulletActionType { Wait, Change_Direction, Change_Speed, Start_Repeat, End_Repeat, Fire, Vertical_Change_Speed, Deactivate }
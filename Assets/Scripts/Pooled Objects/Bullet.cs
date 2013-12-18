using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : PooledGameObject<BulletTag>
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

	public override void Activate (BulletTag param)
	{
		grazed = false;
		rend.sprite = param.sprite;
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
		float deltat = Time.deltaTime;
		for(int i = 0; i < actions.Length; i++)
		{
			switch(actions[i].type)
			{
			case(BulletAction.Type.Wait):
					yield return new WaitForSeconds(actions[i].wait.Value * deltat);
					break;
				case(BulletAction.Type.ChangeDirection):
					if(actions[i].waitForChange)
					{
						yield return ChangeDirection(actions[i]);
					}
					else
					{
						ChangeDirection(actions[i]);
					}
					break;
				case(BulletAction.Type.ChangeSpeed):
					if(actions[i].waitForChange)
					{
						yield return ChangeSpeed(actions[i], false);
					}
					else
					{
						ChangeSpeed(actions[i], false);
					}
					break;
				case(BulletAction.Type.Repeat):
					yield return RunNestedActions(actions[i]);
					break;
				case(BulletAction.Type.Fire):
					if(master != null)
					{
						master.Fire(actions[i].GetSourcePosition(trans.position), trans.rotation, actions[i], param, prevRotation);
					}
					break;
				case(BulletAction.Type.VerticalChangeSpeed):
					if(actions[i].waitForChange)
					{
						yield return ChangeSpeed(actions[i], true);
					}
					else
					{
						ChangeSpeed(actions[i], true);
					}
					break;
				case(BulletAction.Type.Deactivate):
					Deactivate();
					break;
			}
		}
	}

	public IEnumerator RunNestedActions(BulletAction ba)
	{
		float deltat = Time.deltaTime;
		
		float repeatC = Mathf.Floor (ba.repeat.Value);

		for(int i = 0; i < repeatC; i++)
		{
			for(int j = 0; j < ba.nestedActions.Length; j++)
			{
				BulletAction currentAction = ba.nestedActions[j];
				switch(currentAction.type)
				{
					case(BulletAction.Type.Wait):
						yield return new WaitForSeconds(currentAction.wait.Value * deltat);
						break;
					case(BulletAction.Type.ChangeDirection):
						if(currentAction.waitForChange)
						{
							yield return ChangeDirection(actions[i]);
						}
						else
						{
							ChangeDirection(actions[i]);
						}
						break;
					case(BulletAction.Type.ChangeSpeed):
						if(currentAction.waitForChange)
						{
							yield return ChangeSpeed(actions[i], false);
						}
						else
						{
							ChangeSpeed(actions[i], false);
						}
						break;
					case(BulletAction.Type.Repeat):
						yield return RunNestedActions(actions[i]);
						break;
					case(BulletAction.Type.Fire):
						if(master != null)
						{
							master.Fire(actions[i].GetSourcePosition(trans.position), trans.rotation, currentAction, param, prevRotation);
						}
						break;
					case(BulletAction.Type.VerticalChangeSpeed):
						if(currentAction.waitForChange)
						{
							yield return ChangeSpeed(currentAction, true);
						}
						else
						{
							ChangeSpeed(currentAction, true);
						}
						break;
					case(BulletAction.Type.Deactivate):
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

		d = ba.wait.Value * Time.deltaTime;
		
		Quaternion originalRot = trans.localRotation;
		
		// determine offset
		ang = ba.angle.Value;
		
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

		d = ba.wait.Value * Time.deltaTime;	
		
		float originalSpeed = speed;

		newSpeed = ba.speed.UnrankedValue;
		if(ba.speed.rank)
			d += ba.speed.RankValue;
		
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
}
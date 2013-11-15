using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour 
{
	[HideInInspector]
	public Transform bulletTransform;
	[HideInInspector]
	public GameObject bulletObject;
	[HideInInspector]
	public Rigidbody2D bulletRigidBody;

	public BulletPattern master;
	public BulletAction[] actions;
	public PreviousRotationWrapper prevRotation= new PreviousRotationWrapper();

	public float speed = 5.0f;
	public float verticalSpeed = 0.0f;
	public bool useVertical = false;
	public bool grazed;
	public float lifetime = 0.0f;
	public float param = 0.0f;
	public int actionIndex = 0;

	// Use this for initialization
	void Start () 
	{
		gameObject.tag = "Enemy Bullet";
		bulletTransform = transform;
		bulletObject = gameObject;
		bulletRigidBody = rigidbody2D;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		Vector2 targetVelocity = bulletTransform.forward * speed;
		if(useVertical)
		{
			//targetVelocity += (Global.MainCam.up * verticalSpeed);
		}
		Vector2 velocityChange = (targetVelocity - bulletRigidBody.velocity);
		bulletRigidBody.AddForce(velocityChange);
	}

	public void Activate()
	{
		lifetime = 0.0f;
		verticalSpeed = 0.0f;
		useVertical = false;
		prevRotation.prevRotationNull = true;
	}

	public void Deactivate()
	{
		if(bulletObject.activeSelf)
		{
			BulletManager.ReturnBullet(this);
			bulletObject.SetActive(false);
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
						waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
					else
						waitT = actions[actionIndex].waitTime.x;
					if(actions[actionIndex].rankWait)
						waitT += Global.Rank * actions[actionIndex].waitTime.z;
					waitT *= Global.TimePerFrame;
					yield return new WaitForSeconds(waitT);
					break;
				case(BulletActionType.Change_Direction):
					if(actions[actionIndex].waitForChange)
						yield return ChangeDirection(actionIndex);
					else
						ChangeDirection(actionIndex);
					break;
				case(BulletActionType.Change_Speed):
					if(actions[actionIndex].waitForChange)
						yield return ChangeSpeed(actionIndex, false);
					else
						ChangeSpeed(actionIndex, false);
					break;
				case(BulletActionType.Start_Repeat):
					yield return RunNestedActions();
					break;
				case(BulletActionType.Fire):
					if(master != null)
						master.Fire(bulletTransform, actions[actionIndex], param, prevRotation);
					break;
				case(BulletActionType.Vertical_Change_Speed):
					if(actions[actionIndex].waitForChange)
						yield return ChangeSpeed(actionIndex, true);
					else
						ChangeSpeed(actionIndex, true);
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
			repeatC += actions[startIndex].repeatCount.y * Global.Rank;
		repeatC = Mathf.Floor(repeatC);

		for(var y = 0; y < repeatC; y++)
		{
			while(actions[actionIndex].type != BulletActionType.End_Repeat)
			{
				switch(actions[actionIndex].type)
				{
				case(BulletActionType.Wait):
					if(actions[actionIndex].randomWait)
						waitT = Random.Range(actions[actionIndex].waitTime.x, actions[actionIndex].waitTime.y);
					else
						waitT = actions[actionIndex].waitTime.x;
					if(actions[actionIndex].rankWait)
						waitT += Global.Rank * actions[actionIndex].waitTime.z;
					waitT *= Global.TimePerFrame;
					yield return new WaitForSeconds(waitT);
					break;
				case(BulletActionType.Change_Direction):
					if(actions[actionIndex].waitForChange)
						yield return ChangeDirection(actionIndex);
					else
						ChangeDirection(actionIndex);
					break;
				case(BulletActionType.Change_Speed):
					if(actions[actionIndex].waitForChange)
						yield return ChangeSpeed(actionIndex, false);
					else
						ChangeSpeed(actionIndex, false);
					break;
				case(BulletActionType.Start_Repeat):
					yield return RunNestedActions();
					break;
				case(BulletActionType.Fire):
					if(master != null)
						master.Fire(bulletTransform, actions[actionIndex], param, prevRotation);
					break;
				case(BulletActionType.Vertical_Change_Speed):
					if(actions[actionIndex].waitForChange)
						yield return ChangeSpeed(actionIndex, true);
					else
						ChangeSpeed(actionIndex, true);
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
		
		d *= Global.TimePerFrame;
		
		Quaternion originalRot = bulletTransform.localRotation;
		
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
			float dotHeading = Vector3.Dot( bulletTransform.up, Player.playerTransform.position - bulletTransform.position );		
			if(dotHeading > 0)
				dir = -1;
			else
				dir = 1;
			float angleDif = Vector3.Angle(bulletTransform.forward, Player.playerTransform.position - bulletTransform.position);
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
				bulletTransform.localRotation *= newRot;
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
		//all the others just linearly progress to destination rotation
		else if(d > 0)
		{
			while(t < d)
			{
				bulletTransform.localRotation = Quaternion.Slerp(originalRot, newRot, t/d);
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			bulletTransform.localRotation = newRot;
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
		d *= Global.TimePerFrame;	
		
		var originalSpeed = speed;
		
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
}

public class BulletAction : BPAction
{
	public BulletActionType type = BulletActionType.Wait;
	public bool waitForChange = false;
}

public enum BulletActionType { Wait, Change_Direction, Change_Speed, Start_Repeat, End_Repeat, Fire, Vertical_Change_Speed, Deactivate }
using UnityEngine;
using System.Collections;

public class BulletPattern : MonoBehaviour 
{
	
	[HideInInspector] 
	public GameObject BPgameObject;
	[HideInInspector]
	public Transform BPtransform;
	
	FireTag[] fireTags;
	BulletTag[] bulletTags;
	
	float sequenceSpeed = 0.0f;
	
	bool started = false;
	float waitBeforeRepeating = 5.0f;

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
		}
		started = true;

		while(true)
		{

		}
	}

	private void RunFire()
	{
	}

	private void Fire(Transform trans, BPAction action, float param, PreviousRotationWrapper prw)
	{
		float angle, direction, angleDifference, speed;
		BulletTag bt = bulletTags[action.bulletTagIndex - 1];
		Bullet temp = BulletManager.GetBullet();
		if(prw.prevRotationNull)
		{
			prw.prevRotationNull = false;
			prw.previousRotation = temp.bulletTransform.localRotation;
		}
		temp.bulletTransform.position = trans.position;
		temp.bulletTransform.rotation = trans.rotation;
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
			case (DirectionType.PlayerDirected):
			case (DirectionType.Homing):
				Quaternion originalRot = trans.rotation;
				float dotHeading = Vector3.Dot( temp.bulletTransform.up, Player.playerTransform.position - temp.bulletTransform.position );
				
				if(dotHeading > 0)
				{
					direction = -1;
				}
				else
				{
					direction = 1;
				}
				angleDifference = Vector3.Angle(temp.bulletTransform.forward, Player.playerTransform.position - temp.bulletTransform.position);
				temp.bulletTransform.rotation = originalRot * Quaternion.AngleAxis((direction * angleDifference) - angle, Vector3.right);
				break;
				
			case (DirectionType.Absolute):
				temp.bulletTransform.localRotation = Quaternion.Euler(-(angle - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				temp.bulletTransform.localRotation = trans.localRotation * Quaternion.AngleAxis (-angle, Vector3.right);
				break;
				
			case (DirectionType.Sequence):
				temp.bulletTransform.localRotation = prw.previousRotation * Quaternion.AngleAxis (-angle, Vector3.right); 
				break;
		}
		prw.previousRotation = temp.bulletTransform.localRotation;
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

		if(a.passPassedParam)
		{
			temp.param = param;
		}
		temp.master = this;
		temp.Activate();
	}
}

public enum DirectionType { PlayerDirected, Homing, Absolute, Relative, Sequence }

public enum FireActionType { Wait, Fire, CallFireTag, StartRepeat, EndRepeat }

public class FireTag
{
	public float param = 0.0f;
	public PreviousRotationWrapper previousRotation;

}

public class BulletTag
{
	public Vector3 speed;
	public bool randomSpeed = false;
	public bool rankSpeed = false;
	public int prefabIndex = 0;
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
	public FireActionType type = FireActionType.Wait; 
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Bullet : GameObjectManager.PooledGameObject<Bullet, BulletTag>
{
	[HideInInspector]
	public AttackPattern master;
	private BulletTag bulletTag;
	[HideInInspector]
	public GameObject prefab;
	[HideInInspector]
	public RotationWrapper prevRotation = new RotationWrapper();
	
	[HideInInspector]
	public Vector2 velocity = new Vector2 (5.0f, 0.0f);
	[HideInInspector]
	public bool useVertical = false;
	[HideInInspector]
	public bool grazed;
	[HideInInspector]
	public float param = 0.0f;
	[HideInInspector]
	public Collider2D col;
	public SpriteRenderer rend;

	public override void Awake ()
	{
		base.Awake ();
		col = collider2D;
		rend = (SpriteRenderer)renderer;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		Vector3 movementVector = Transform.up * velocity.x * Time.fixedDeltaTime;
		Debug.DrawRay (Transform.position, Transform.up * velocity.x * Time.deltaTime, Color.red);
		if(useVertical)
		{
			movementVector += (Vector3.up * velocity.y * Time.fixedDeltaTime);
		}
		Transform.position += movementVector;
	}

	public override void Activate (BulletTag param)
	{
		grazed = false;
		if(prefab != param.prefab)
		{
			prefab = param.prefab;
			SpriteRenderer sp = prefab.renderer as SpriteRenderer;
			rend.color = sp.color;
			rend.sprite = sp.sprite;
		}
		bulletTag = param;
	}

	public override void LateActivate ()
	{
		bulletTag.Run (this);
	}

	public void Deactivate()
	{
		if(GameObject.activeSelf)
		{
			GameObjectManager.Bullets.Return(this);
			master.bulletsInPlay.Remove(this);
		}
	}

	public IEnumerator ChangeDirection(Bullet.Action action)
	{
		Quaternion newRot = Quaternion.identity;
		float t = 0.0f;
		float d = action.wait.Value * Time.deltaTime;
		
		Quaternion originalRot = Transform.localRotation;
		IEnumerator pause;
		
		// determine offset
		float ang = action.angle.Value;
		
		//and set rotation depending on angle
		switch(action.direction)
		{
			case (DirectionType.TargetPlayer):
//				float dotHeading = Vector3.Dot( bullet.Transform.up, Player.PlayerTransform.position - bullet.Transform.position );		
//				if(dotHeading > 0)
//					dir = -1;
//				else
//					dir = 1;
//				float angleDif = Vector3.Angle(bullet.Transform.forward, Player.PlayerTransform.position - bullet.Transform.position);
//				newRot = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right); 
				break;
				
			case (DirectionType.Absolute):
				newRot = Quaternion.Euler(-(ang - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				newRot = originalRot * Quaternion.AngleAxis(-ang, Vector3.right);
				break;
		}
		
		//Sequence has its own thing going on, continually turning a set amount until time is up
		if(action.direction == DirectionType.Sequence)
		{
			newRot = Quaternion.AngleAxis (-ang, Vector3.right); 
			
			while(t < d)
			{
				Transform.localRotation *= newRot;
				t += Time.deltaTime;

			}
		}
		//all the others just linearly progress to destination rotation
		else if(d > 0)
		{
			while(t < d)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				Transform.localRotation = Quaternion.Slerp(originalRot, newRot, t/d);
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}
			
			Transform.localRotation = newRot;
		}
	}

	public IEnumerator ChangeSpeed(Bullet.Action action)
	{
		if(action.isVertical)
		{
			useVertical = true;
		}
		float currentTime = 0.0f;
		float totalTime = action.wait.Value * Time.deltaTime;
		float originalSpeed = velocity.x;
		float currentSpeed = originalSpeed;
		IEnumerator pause;
		
		float newSpeed = action.speed.UnrankedValue;
		if(action.speed.rank)
		{
			totalTime += action.speed.RankValue;
		}
		
		if(totalTime > 0)
		{
			while(currentTime < totalTime)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				currentSpeed = Mathf.Lerp(originalSpeed, newSpeed, currentTime/totalTime);
				if(action.isVertical) 
				{
					velocity.y = currentSpeed;
				}
				else 
				{
					velocity.x = currentSpeed;
				}
				currentTime += Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
		}
		
		if(action.isVertical)
		{
			velocity.y = newSpeed;
		}
		else 
		{
			velocity.x = newSpeed;
		}
	}

	public void Cancel()
	{
		GameObjectManager.Pickups.Spawn (Transform.position, Pickup.Type.PointValue);
		Deactivate();
	}

	[System.Serializable]
	public class Action : AttackPatternAction<Bullet.Action, Bullet.Action.Type>
	{
		public enum Type { Wait, ChangeDirection, ChangeSpeed, ChangeScale, Repeat, Fire, Deactivate }
		[SerializeField]
		public bool isVertical;
		
		#if UNITY_EDITOR
		public override void ActionGUI(params object[] param)
		{
			type = (Type)EditorGUILayout.EnumPopup("Type", type);
			AttackPattern attackPattern = param [0] as AttackPattern;
			switch(type)
			{
				case Type.ChangeDirection:
					direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", direction);
					angle = AttackPattern.Property.EditorGUI("Angle", angle, false);
					wait = AttackPattern.Property.EditorGUI("Time", wait, false);
					waitForFinish = EditorGUILayout.Toggle("Wait To Finish", waitForFinish);
					break;
				case Type.ChangeSpeed:
					speed = AttackPattern.Property.EditorGUI("Speed", speed, false);
					wait = AttackPattern.Property.EditorGUI("Time", wait, false);
					waitForFinish = EditorGUILayout.Toggle("Wait To Finish", waitForFinish);
					break;
				case Type.Wait:
					wait = AttackPattern.Property.EditorGUI ("Wait", wait, false);
					break;
				case Type.Repeat:
					SharedAction.Repeat.ActionGUI<Bullet.Action, Bullet.Action.Type> (this, param);
					break;
				case Type.Fire:
					SharedAction.Fire.ActionGUI<Bullet.Action, Bullet.Action.Type>(this, attackPattern);
					break;
			}
		}
		
		public override void DrawGizmosImpl (Bullet.Action previous)
		{
			
		}
		#endif
		
		public override IEnumerator Execute(params object[] param)
		{
			Bullet bullet = param [0] as Bullet;
			AttackPattern attackPattern = param [1] as AttackPattern;
			Enemy master = parent as Enemy;
			IEnumerator pause, actionEnumerator;
			switch(type)
			{
				case Type.ChangeDirection:
					if(waitForFinish)
					{
						yield return bullet.StartCoroutine(bullet.ChangeDirection(this));
					}
					else
					{
						bullet.StartCoroutine(bullet.ChangeDirection(this));
					}
					break;
				case Type.ChangeSpeed:
					if(waitForFinish)
					{
						yield return bullet.StartCoroutine(bullet.ChangeSpeed(this));
					}
					else
					{
						bullet.StartCoroutine(bullet.ChangeSpeed(this));
					}
					break;
				case Type.Repeat:
					int repeatC = Mathf.FloorToInt(repeat.Value);
					for(int j = 0; j < repeatC; j++)
					{
						foreach(Action action in nestedActions)
						{
							pause = Global.WaitForUnpause();
							while(pause.MoveNext())
							{
								yield return pause.Current;
							}
							actionEnumerator = action.Execute(param[0], param[1]);
							while(actionEnumerator.MoveNext())
							{
								yield return actionEnumerator.Current;
							}
						}
					}
					break;
				case Type.Wait:
					yield return new WaitForSeconds(wait.Value);
					break;
				case Type.Fire:
					attackPattern.Fire<Bullet.Action, Bullet.Action.Type>(this, master, bullet.Transform.position, bullet.Transform.rotation, bullet.param, bullet.prevRotation);
					break;
			}
		}
	}
}
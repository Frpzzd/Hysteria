using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Actions;

namespace DanmakuEngine.Core
{
	[System.Serializable]
	[RequireComponent(typeof(CircleCollider2D))]
	[RequireComponent(typeof(BoxCollider2D))]
	public sealed class Bullet : PooledGameObject<Bullet, BulletPool>
	{
		[System.NonSerialized]
		public AbstractAttackPattern master;
		[System.NonSerialized]
		private BulletTag bulletTag;
		[HideInInspector]
		public GameObject prefab;
		[HideInInspector]
		public SequenceWrapper prevRotation = new SequenceWrapper();
		
		[HideInInspector]
		public Vector2 velocity = new Vector2 (5.0f, 0.0f);
		[HideInInspector]
		public bool useVertical = false;
		[HideInInspector]
		public bool grazed;
		[HideInInspector]
		public float param = 0.0f;
		[HideInInspector]
		public CircleCollider2D cir;
		[HideInInspector]
		public BoxCollider2D box;
		[HideInInspector]
		public SpriteRenderer rend;
		[HideInInspector]
		public bool fake = false;
		
		public override void Awake ()
		{
			base.Awake ();
			cir = GetComponent<CircleCollider2D> ();
			box = GetComponent<BoxCollider2D> ();
			rend = renderer as SpriteRenderer;
		}

		void OnEnable()
		{
			if(bulletTag != null && master != null)
			{
				StartCoroutine (bulletTag.Run (this, master));
			}
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
			Color rendColor = rend.color;
			rendColor.a = (fake) ? 0.5f : 1.0f;
			rend.color = rendColor;
		}
		
		public override void Activate (object[] param)
		{
			grazed = false;
			BulletTag bt = param [0] as BulletTag;
			prefab = bt.prefab;
			SpriteRenderer sp = prefab.renderer as SpriteRenderer;
			rend.color = sp.color;
			rend.sprite = sp.sprite;
			bulletTag = bt;
			Transform.localScale = prefab.transform.localScale;
			if(bulletTag.overwriteColor)
			{
				rend.color = bulletTag.newColor;
			}
			Collider2D col = prefab.collider2D;
			if(col is CircleCollider2D)
			{
				CircleCollider2D c = col as CircleCollider2D;
				cir.radius = c.radius;
				cir.center = c.center;
				cir.enabled = true;
				box.enabled = false;
			}
			else if(col is BoxCollider2D)
			{
				BoxCollider2D b = col as BoxCollider2D;
				box.size = b.size;
				box.center = b.center;
				box.enabled = true;
				cir.enabled = false;
			}
			else
			{
				throw new UnityException("Prefab has no circle/box collider on it");
			}
		}
		
		public IEnumerator ChangeDirection(Action action)
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
				Vector3 r = Player.PlayerTransform.position - Transform.position;
				newRot = Quaternion.Euler(0, 0, ang + Mathf.Atan2(r.y, r.x) * (180f/Mathf.PI) - 90);
				break;
				
			case (DirectionType.Absolute):
				newRot = Quaternion.Euler(0, 0, ang);
				break;
				
			case (DirectionType.Relative):
				newRot = Quaternion.Euler(0, 0, Transform.rotation.eulerAngles.z + ang);
				break;
			}
			
			//Sequence has its own thing going on, continually turning a set amount until time is up
//			if(action.direction == DirectionType.Sequence)
//			{
//				newRot = Quaternion.AngleAxis (-ang * Time.fixedDeltaTime, Vector3.right); 
//				
//				while(t < d)
//				{
//					Transform.localRotation *= newRot;
//					t += Time.fixedDeltaTime;
//					yield return new WaitForFixedUpdate();
//				}
//			}
			//TODO:Fix this
			//all the others just linearly progress to destination rotation
			if(d > 0)
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
		
		public IEnumerator ChangeSpeed(Action action)
		{
			if(action.isVertical)
			{
				useVertical = true;
			}
			float currentTime = 0.0f;
			float totalTime = action.wait.Value;
			float originalSpeed = velocity.x;
			float currentSpeed = originalSpeed;
			IEnumerator pause;
			
			float newSpeed = action.speed.Value;
			
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
			PickupPool.Spawn (Transform.position, Pickup.Type.PointValue);
			Deactivate();
		}

		[System.Serializable]
		public class Action : AttackPatternAction<Action, Action.Type>
		{
			public enum Type { Wait, ChangeDirection, ChangeSpeed, ChangeScale, Repeat, Fire, Deactivate, ChangeFake}
			[SerializeField]
			public bool isVertical;
			
			public override IEnumerator Execute(params object[] param)
			{
				Bullet bullet = param [0] as Bullet;
				ActionAttackPattern attackPattern = param [1] as ActionAttackPattern;
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
					attackPattern.Fire<Action, Action.Type>(this, master, bullet.Transform.position, bullet.Transform.rotation, bullet.param, bullet.prevRotation);
					break;
				case Type.ChangeFake:
					bullet.fake = fake;
					break;
				}
			}

			protected override void DrawHandlesImpl (Action previous)
			{
			}
		}

		public void Deactivate()
		{
			if(GameObject.activeSelf)
			{
				BulletPool.Return(this);
			}
		}
	}
}

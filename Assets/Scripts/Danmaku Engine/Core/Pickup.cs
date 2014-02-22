using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DanmakuEngine.Core
{
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(Collider2D))]
	public class Pickup : PooledGameObject<Pickup, PickupPool>
	{
		public enum State { Normal, AutoCollect, ProximityCollect }
		public enum Type { Power, Point, PointValue, Life }
		[System.NonSerialized]
		public State state;
		[System.NonSerialized]
		public Type type;
		[System.NonSerialized]
		public SpriteRenderer render;
		[System.NonSerialized]
		public float currentVelocity;
		
		public override void Awake()
		{
			base.Awake();
			render = renderer as SpriteRenderer;
		}
		
		public override void Activate (object[] param)
		{
			state = Global.defaultPickupState;
			PickupPool pi = PickupPool.PoolInstance;
			currentVelocity = pi.InitialVelocity;
			switch((Type)param[0])
			{
			case Type.Power:
				render.color = pi.powerColor;
				render.sprite = pi.powerSprite;
				break;
			case Type.Point:
				render.color = pi.pointColor;
				render.sprite = pi.pointSprite;
				break;
			case Type.PointValue:
				render.color = pi.pointValueColor;
				render.sprite = pi.pointValueSprite;
				state = State.AutoCollect;
				break;
			case Type.Life:
				render.color = pi.lifeColor;
				render.sprite = pi.lifeSprite;
				break;
			}
		}

		void OnEnable()
		{
			StartCoroutine (RotateOnce());
		}
		
		void Update()
		{
			float deltat = Time.deltaTime;
			Transform.position += Vector3.up * currentVelocity * deltat;
			if(currentVelocity > PickupPool.PoolInstance.MaximumDownwardVelocity)
			{
				currentVelocity += PickupPool.PoolInstance.Acceleration * deltat;
				if(currentVelocity < PickupPool.PoolInstance.MaximumDownwardVelocity)
				{
					currentVelocity = PickupPool.PoolInstance.MaximumDownwardVelocity;
				}
			}
			if(currentVelocity < 0 || type == Type.PointValue)
			{
				switch(state)
				{
					case State.AutoCollect:
						Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, PickupPool.PoolInstance.AutoCollectSpeed * deltat);
						break;
					case State.ProximityCollect:
						Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, PickupPool.PoolInstance.ProximityCollectSpeed * deltat);
						break;
					default:
						break;
				}
			}
		}
		
		public IEnumerator RotateOnce()
		{
			float rotationAmount = 0f;
			IEnumerator pause;
			while(rotationAmount < 1)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				rotationAmount += ((float)PickupPool.PoolInstance.RotationSpeed / 360f) * Time.fixedDeltaTime;
				Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,360,0)), rotationAmount);
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
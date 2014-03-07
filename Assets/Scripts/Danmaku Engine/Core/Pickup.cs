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
		public enum Type : int { Power = 0, Point, PointValue, Life, BigPower, MaxPower, BigPoint}
		[System.NonSerialized]
		public State state;
		[System.NonSerialized]
		public Type type;
		[System.NonSerialized]
		public SpriteRenderer render;
		[System.NonSerialized]
		public float currentVelocity;
		[System.NonSerialized]
		public Vector3 defaultScale;
		
		public override void Awake()
		{
			base.Awake();
			render = renderer as SpriteRenderer;
			defaultScale = Transform.localScale;
		}
		
		public override void Activate (object[] param)
		{
			state = Global.defaultPickupState;
			PickupPool pi = PickupPool.PoolInstance;
			currentVelocity = pi.InitialVelocity;
			type = (Type)param [0];
			switch(type)
			{
			case Type.Power:
				render.color = pi.powerColor;
				render.sprite = pi.powerSprite;
				Transform.localScale = defaultScale;
				break;
			case Type.Point:
				render.color = pi.pointColor;
				render.sprite = pi.pointSprite;
				Transform.localScale = defaultScale;
				break;
			case Type.PointValue:
				render.color = pi.pointValueColor;
				render.sprite = pi.pointValueSprite;
				Transform.localScale = defaultScale;
				state = State.AutoCollect;
				break;
			case Type.Life:
				render.color = pi.lifeColor;
				render.sprite = pi.lifeSprite;
				Transform.localScale = defaultScale;
				break;
			case Type.BigPower:
				render.color = pi.powerColor;
				render.sprite = pi.powerSprite;
				Transform.localScale = pi.bigPowerScale * defaultScale;
				break;
			case Type.MaxPower:
				render.color = pi.fullPowerColor;
				render.sprite = pi.powerSprite;
				Transform.localScale = pi.bigPowerScale * defaultScale;
				break;
			case Type.BigPoint:
				render.color = pi.pointColor;
				render.sprite = pi.pointSprite;
				Transform.localScale = pi.bigPowerScale * defaultScale;
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
			if(currentVelocity < 0)
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
				Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,0,179)), rotationAmount);
				yield return new WaitForFixedUpdate();
			}
			Transform.rotation = Quaternion.identity;
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Pickup : GameObjectManager.PooledGameObject<Pickup, Pickup.Type>
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

	public override void Activate (Type param)
	{
		state = (param == Type.PointValue) ? State.AutoCollect : Global.defaultPickupState;
		currentVelocity = GameObjectManager.Pickups.InitialVelocity;
		type = param;
	}

	public override void LateActivate()
	{
		StartCoroutine(RotateOnce());
	}

	void Update()
	{
		float deltat = Time.deltaTime;
		Transform.position += Vector3.up * currentVelocity * deltat;
		if(currentVelocity > GameObjectManager.Pickups.MaximumDownwardVelocity)
		{
			currentVelocity += GameObjectManager.Pickups.Acceleration * deltat;
			if(currentVelocity < GameObjectManager.Pickups.MaximumDownwardVelocity)
			{
				currentVelocity = GameObjectManager.Pickups.MaximumDownwardVelocity;
			}
		}
		if(currentVelocity < 0 || type == Type.PointValue)
		{
			switch(state)
			{
				case State.AutoCollect:
					Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, GameObjectManager.Pickups.AutoCollectSpeed * deltat);
					break;
				case State.ProximityCollect:
					Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, GameObjectManager.Pickups.ProximityCollectSpeed * deltat);
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
			rotationAmount += ((float)GameObjectManager.Pickups.RotationSpeed / 360f) * Time.fixedDeltaTime;
			Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,360,0)), rotationAmount);
			yield return new WaitForFixedUpdate();
		}
	}
}
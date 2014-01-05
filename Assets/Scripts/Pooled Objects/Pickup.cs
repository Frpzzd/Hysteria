using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Pickup : GameObjectManager.PooledGameObject<Pickup, Pickup.Type>
{
	public enum State { Normal, AutoCollect, ProximityCollect }
	public enum Type { Power, Point, PointValue, Life, Bomb }
	public State state;
	[System.NonSerialized]
	public Type type;
	public SpriteRenderer render;
	public float initialVelocity;
	[System.NonSerialized]
	public float currentVelocity;
	public float maximumDownwardVelocity;
	public float acceleration;
	public float autoCollectSpeed;
	public float proximityCollectSpeed;
	public float rotationSpeed;

	public override void Awake()
	{
		base.Awake();
		render = renderer as SpriteRenderer;
	}

	public override void Activate (Type param)
	{
		state = (param == Type.PointValue) ? State.AutoCollect : Global.defaultPickupState;
		currentVelocity = initialVelocity;
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
		if(currentVelocity > maximumDownwardVelocity)
		{
			currentVelocity += acceleration * deltat;
			if(currentVelocity < maximumDownwardVelocity)
			{
				currentVelocity = maximumDownwardVelocity;
			}
		}
		if(currentVelocity < 0 || type == Type.PointValue)
		{
			switch(state)
			{
				case State.AutoCollect:
					Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, autoCollectSpeed * deltat);
					break;
				case State.ProximityCollect:
					Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, proximityCollectSpeed * deltat);
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
			rotationAmount += Time.fixedDeltaTime;
			Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,360,0)), rotationAmount);
			yield return new WaitForFixedUpdate();
		}
	}
}
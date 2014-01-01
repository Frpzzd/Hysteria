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
	private Material mat;
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
		mat = renderer.material;
	}

	public override void Activate (Type param)
	{
		state = Global.defaultPickupState;
		currentVelocity = initialVelocity;
		type = param;
		switch(type)
		{
		case Type.Point:
			mat.color = Color.blue;
			break;
		case Type.Power:
			mat.color = Color.red;
			break;
		case Type.Bomb:
			mat.color = Color.green;
			break;
		case Type.Life:
			mat.color = Color.magenta;
			break;
		}
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
		if(currentVelocity < 0)
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
		while(rotationAmount < 1)
		{
			yield return StartCoroutine(Global.WaitForUnpause());
			rotationAmount += Time.fixedDeltaTime;
			Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,360,0)), rotationAmount);
			yield return new WaitForFixedUpdate();
		}
	}
}
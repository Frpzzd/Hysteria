using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PickupType { Power, Point, PointValue, Life, Bomb }

public class Pickup : GameObjectManager.PooledGameObject<Pickup, PickupType>
{
	public enum PickupState { Normal, AutoCollect, ProximityCollect }
	public PickupState state;
	public PickupType type;
	private Material mat;
	public float initialVelocity;
	[HideInInspector]
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

	public override void Activate (PickupType param)
	{
		state = Global.defaultPickupState;
		currentVelocity = initialVelocity;
		type = param;
		switch(type)
		{
		case PickupType.Point:
			mat.color = Color.blue;
			break;
		case PickupType.Power:
			mat.color = Color.red;
			break;
		case PickupType.Bomb:
			mat.color = Color.green;
			break;
		case PickupType.Life:
			mat.color = Color.magenta;
			break;
		}
		RotateOnce();
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
			case PickupState.AutoCollect:
				Transform.position = Vector3.MoveTowards(Transform.position, Player.PlayerTransform.position, autoCollectSpeed * deltat);
				break;
			case PickupState.ProximityCollect:
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
		while(rotationAmount > 1)
		{
			rotationAmount += Time.fixedDeltaTime;
			Transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(new Vector3(0,360,0)), rotationAmount);
			yield return new WaitForFixedUpdate();
		}
	}
}
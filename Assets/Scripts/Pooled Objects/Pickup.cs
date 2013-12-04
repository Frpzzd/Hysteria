using UnityEngine;
using System.Collections.Generic;

public enum PickupType { Power, Point, PointValue, Life, Bomb }

public class Pickup : PooledGameObject<PickupType>
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
	}

	void Update()
	{
		float deltat = Time.deltaTime;
		if(state == PickupState.AutoCollect)
		{
			trans.position = Vector3.MoveTowards(trans.position, Player.playerTransform.position, autoCollectSpeed * deltat);
		}
		else
		{
			trans.position += Vector3.up * currentVelocity * deltat;
			if(currentVelocity > maximumDownwardVelocity)
			{
				currentVelocity += acceleration * deltat;
				if(currentVelocity < maximumDownwardVelocity)
				{
					currentVelocity = maximumDownwardVelocity;
				}
			}
			if(state == PickupState.ProximityCollect)
			{
				trans.position = Vector3.MoveTowards(trans.position, Player.playerTransform.position, proximityCollectSpeed * deltat);
			}
		}
	}
}
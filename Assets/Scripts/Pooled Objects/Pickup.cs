using UnityEngine;
using System.Collections.Generic;

public enum PickupType { Power, Point, PointValue, Life, Bomb }

public class Pickup : PooledGameObject
{
	public enum PickupState { Normal, AutoCollect, ProximityCollect }
	public PickupState state;
	public PickupType type;
	public float initialVelocity;
	[HideInInspector]
	public float currentVelocity;
	public float maximumDownwardVelocity;
	public float acceleration;
	public float autoCollectSpeed;
	public float proximityCollectSpeed;

	public override void Activate()
	{
		state = Global.defaultPickupState;
		currentVelocity = initialVelocity;
	}

	void Update()
	{
		switch(state)
		{
			case PickupState.Normal:
				trans.Translate(0, currentVelocity * Time.deltaTime, 0);
				if(currentVelocity > maximumDownwardVelocity)
				{
					currentVelocity += acceleration * Time.deltaTime;
					if(currentVelocity < maximumDownwardVelocity)
					{
						currentVelocity = maximumDownwardVelocity;
					}
				}
				break;
		case PickupState.AutoCollect:
				trans.position = Vector3.MoveTowards(trans.position, Player.playerTransform.position, autoCollectSpeed * Time.deltaTime);
				break;
		case PickupState.ProximityCollect:
				trans.position = Vector3.MoveTowards(trans.position, Player.playerTransform.position, proximityCollectSpeed * Time.deltaTime);
				break;
		}
	}

}
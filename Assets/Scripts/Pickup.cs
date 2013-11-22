using UnityEngine;
using System.Collections.Generic;

public enum PickupType { Power, Point, PointValue, Life, Bomb }

public class Pickup : PooledGameObject
{
	public enum PickupState { Normal, AutoCollect, ProximityCollect }
	public PickupState state;
	public PickupType type;
	public float initialVelocity;
	public float currentVelocity;
	public float maximumDownwardVelocity;
	public float acceleration;
	public float autoCollectSpeed;
	private Rigidbody2D rigBody2D;

	[HideInInspector]
	public Transform pickupTransform;

	void Start()
	{
		gameObject.tag = "Pickup";
		pickupTransform = transform;
		rigBody2D = rigidbody2D;
	}

	public override void Activate()
	{
		state = PickupState.Normal;
	}

	void FixedUpdate()
	{
		switch(state)
		{
			case PickupState.Normal:
				pickupTransform.Translate(0, currentVelocity, 0);
				currentVelocity = (currentVelocity >= maximumDownwardVelocity) ? currentVelocity + acceleration : currentVelocity;
				break;
			case PickupState.AutoCollect:
				pickupTransform.Translate(Vector3.MoveTowards(pickupTransform.localPosition, Player.playerTransform.localPosition, autoCollectSpeed));
				break;
			case PickupState.ProximityCollect:
				pickupTransform.Translate(Vector3.MoveTowards(pickupTransform.localPosition, Player.playerTransform.localPosition, autoCollectSpeed));
				break;
		}
	}

	void OnTrigger(Collider col)
	{
		switch(col.gameObject.tag)
		{
			case "Player":
				switch(type)
				{
					case PickupType.Point:
						Global.Score += 1; 		//Filler for now, work out scoring function later
						break;
					case PickupType.PointValue:
						Global.PointValue += 10;
						break;
					case PickupType.Power:
						Player.instance.power += 0.01f;
						Global.Score += 1;		//Reward a little few points of score to the player for power pickups
						break;
					case PickupType.Life:
						Player.instance.lives++;
						break;
					case PickupType.Bomb:
						Player.instance.bombs++;
						break;
				}
				GameObjectManager.Pickups.Return(this);
				return;
			case "Proximity Collection Hitbox":
				state = (state == PickupState.Normal) ? PickupState.ProximityCollect : state;
				break;
			default:
				break;
		}
	}

}
using UnityEngine;
using System.Collections.Generic;

public enum PickupType { Power, Point, PointValue, Life, Bomb }

public class Pickup : MonoBehaviour 
{
	public PickupType type;
	public bool autoCollect;
	public float initialVelocity;
	public float currentVelocity;
	public float maximumDownwardVelocity;
	public float acceleration;
	public float autoCollectSpeed;

	[HideInInspector]
	public Transform pickupTransform;

	void Start()
	{
		gameObject.tag = "Pickup";
		pickupTransform = transform;
	}

	void Update()
	{
		if(autoCollect)
		{
			pickupTransform.Translate(Vector3.MoveTowards(pickupTransform.localPosition, Player.playerTransform.localPosition, autoCollectSpeed));
		}
		else
		{
			pickupTransform.Translate(0, currentVelocity, 0);
			currentVelocity = (currentVelocity >= maximumDownwardVelocity) ? currentVelocity + acceleration : currentVelocity;
		}
	}
}


public static class PickupPool
{
	private static Stack<Pickup> pickupPool;

	static PickupPool()
	{
		pickupPool = new Stack<Pickup>();
	}

	public static void spawnPickup(Vector3 location, PickupType type)
	{
		Pickup newPickup = pickupPool.Pop();
		if(newPickup == null)
		{

		}
		newPickup.pickupTransform.localPosition = location;
		newPickup.type = type;
	}
}
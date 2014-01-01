using UnityEngine;
using System;
using System.Collections;

public class ProximityCollectHitbox : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Pickup pickup = other.gameObject.GetComponent<Pickup> ();
		if(pickup == null)
		{
			Debug.LogError("Cannot Find Pickup in GameObject that Collided with Proximity Collect Hitbox: " + other.gameObject.name);
		}
		else
		{
			if(pickup.state != Pickup.State.AutoCollect)
			{
				pickup.state = Pickup.State.ProximityCollect;
			}
		}
	}
}

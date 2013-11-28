using UnityEngine;
using System;
using System.Collections;

public class ProximityCollectHitbox : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D col)
	{
		Pickup pickup = col.gameObject.GetComponent<Pickup> ();
		if(pickup == null)
		{
			throw new NullReferenceException("Cannot Find Bullet in GameObject that Collided with Proximity Collect Hitbox");
		}
		else
		{
			if(pickup.state != Pickup.PickupState.AutoCollect)
			{
				pickup.state = Pickup.PickupState.ProximityCollect;
			}
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		Pickup pickup = col.gameObject.GetComponent<Pickup> ();
		if(pickup == null)
		{
			throw new NullReferenceException("Cannot Find Pickup in GameObject that Collided with Proximity Collect Hitbox");
		}
		else
		{
			if(pickup.state != Pickup.PickupState.AutoCollect)
			{
				pickup.state = Pickup.PickupState.Normal;
			}
		}
	}
}

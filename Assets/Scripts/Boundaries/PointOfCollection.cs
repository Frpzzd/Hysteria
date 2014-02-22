using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;

public class PointOfCollection : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			Global.defaultPickupState = Pickup.State.AutoCollect;
			PickupPool.AutoCollectAll();
		}
		else
		{
			Debug.LogError("Something other than a player collided with the PoC: " + other.gameObject.name);
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			Global.defaultPickupState = Pickup.State.Normal;
		}
	}
}

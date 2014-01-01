using UnityEngine;
using System.Collections;

public class PointOfCollection : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			Global.defaultPickupState = Pickup.State.AutoCollect;
			foreach(Pickup pickup in GameObjectManager.Pickups.All)
			{
				pickup.state = Pickup.State.AutoCollect;
			}
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

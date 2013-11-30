using UnityEngine;
using System.Collections;

public class PointOfCollection : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log ("Success");
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			Global.defaultPickupState = Pickup.PickupState.AutoCollect;
			foreach(Pickup pickup in GameObjectManager.Pickups.All)
			{
				pickup.state = Pickup.PickupState.AutoCollect;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		Debug.Log ("Success");
		Player p = other.gameObject.GetComponent<Player> ();
		if(p != null)
		{
			Global.defaultPickupState = Pickup.PickupState.Normal;
		}
	}
}

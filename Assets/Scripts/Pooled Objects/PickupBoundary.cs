using UnityEngine;
using System.Collections;

public class PickupBoundary : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		GameObject go = other.gameObject;
		Pickup p = go.GetComponent<Pickup> ();
		if(p != null)
		{
			GameObjectManager.Pickups.Return(p);
		}
	}
}

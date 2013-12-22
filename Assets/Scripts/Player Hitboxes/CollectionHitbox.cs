using UnityEngine;
using System.Collections;

public class CollectionHitbox : PlayerHitbox 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Pickup p = other.gameObject.GetComponent<Pickup> ();
		if(p != null)
		{
			Player.Pickup (p.type);
			GameObjectManager.Pickups.Return(p);
		}
	}
}

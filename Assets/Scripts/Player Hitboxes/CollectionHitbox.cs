using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;

public class CollectionHitbox : PlayerHitbox 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		Pickup p = other.gameObject.GetComponent<Pickup> ();
		if(p != null)
		{
			Player.PickupItem (p.type);
			PickupPool.Return(p);
		}
	}
}

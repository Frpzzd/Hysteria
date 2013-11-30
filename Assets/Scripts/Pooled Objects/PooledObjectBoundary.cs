using UnityEngine;
using System.Collections;

public class PooledObjectBoundary : MonoBehaviour 
{
	void OnTriggerEnter2D(Collider2D other)
	{
		GameObject go = other.gameObject;
		Bullet b = go.GetComponent<Bullet> ();
		Pickup p = go.GetComponent<Pickup> ();
		PlayerShot ps = go.GetComponent<PlayerShot> ();
		if(b != null)
		{
			GameObjectManager.Bullets.Return(b);
		}
		if(p != null)
		{
			GameObjectManager.Pickups.Return(p);
		}
		if(ps != null)
		{
			if(ps.mainShot)
			{
				GameObjectManager.MainPlayerShots.Return(ps);
			}
			else
			{
				GameObjectManager.OptionPlayerShots.Return(ps);
			}
		}
	}
}

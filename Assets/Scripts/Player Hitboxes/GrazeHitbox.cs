using UnityEngine;
using System;
using System.Collections;
using DanmakuEngine.Core;

public class GrazeHitbox : PlayerHitbox
{
	void OnTriggerExit2D(Collider2D col)
	{
		Bullet bullet = col.gameObject.GetComponent<Bullet> ();
		if(bullet == null)
		{
			throw new NullReferenceException("Cannot Find Bullet in Object that Collided with Graze Hitbox");
		}
		else
		{
			master.Graze(bullet);
		}
	}
}

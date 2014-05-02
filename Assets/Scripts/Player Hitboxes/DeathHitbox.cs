using UnityEngine;
using DanmakuEngine.Core;

[RequireComponent(typeof(SpriteRenderer))]
public class DeathHitbox : PlayerHitbox
{
	public float rotationSpeed = 5.0f;

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		Bullet b = col.gameObject.GetComponent<Bullet> ();
		if(b != null && !b.fake)
		{
			master.Die ();
		}
	}
}
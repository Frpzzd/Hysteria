using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;

public class BulletBoundary : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		GameObject go = other.gameObject;
		Bullet b = go.GetComponent<Bullet> ();
		if(b != null)
		{
			b.Deactivate();
		}
	}
}

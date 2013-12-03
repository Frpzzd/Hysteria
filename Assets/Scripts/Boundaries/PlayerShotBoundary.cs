using UnityEngine;
using System.Collections;

public class PlayerShotBoundary : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		GameObject go = other.gameObject;
		PlayerShot ps = go.GetComponent<PlayerShot> ();
		if(ps != null)
		{
			GameObjectManager.PlayerShots.Return(ps);
		}
	}
}

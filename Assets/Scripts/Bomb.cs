using UnityEngine;
using System.Collections;

public abstract class Bomb : MonoBehaviour 
{
	void OnTrigger(Collider col)
	{
		if(col.gameObject.CompareTag("Enemy Bullet"))
		{
			col.gameObject.GetComponent<Bullet>().Cancel();
		}
	}
}

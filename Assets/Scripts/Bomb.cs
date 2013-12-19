using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour 
{
	[HideInInspector]
	public float Duration;

	public bool Active
	{
		get { return gameObject.activeSelf; }
		set { gameObject.SetActive (value); }
	}

	void OnTrigger(Collider col)
	{
		if(col.gameObject.CompareTag("Enemy Bullet"))
		{
			col.gameObject.GetComponent<Bullet>().Cancel();
		}
	}
}

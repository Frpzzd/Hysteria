using UnityEngine;
using System.Collections;

public class PlayerHitboxHandler : MonoBehaviour 
{
	[HideInInspector]
	public Player master;
	[HideInInspector]
	public Renderer hitboxRenderer;

	void Start()
	{
		hitboxRenderer = renderer;
	}

	void OnTrigger(Collider col)
	{
		if(col.gameObject.CompareTag("Enemy Bullet"))
		{
			master.Die();
		}
	}
}

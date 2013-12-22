using UnityEngine;

public class DeathHitbox : PlayerHitbox
{
	public float rotationSpeed = 5.0f;

	void Update () 
	{
		
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		master.Die ();
	}
}

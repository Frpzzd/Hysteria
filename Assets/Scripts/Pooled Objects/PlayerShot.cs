using UnityEngine;
using System.Collections;

public class PlayerShot : PooledGameObject<bool>
{
	public bool mainShot;
	public float speed;

	public override void Activate (bool param)
	{
		mainShot = param;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(mainShot)
		{
			trans.position += Vector3.up * speed * Time.deltaTime;
		}
	}
}

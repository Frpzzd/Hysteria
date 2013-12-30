using UnityEngine;
using System.Collections;

public class PickupTest : TestScript
{
	public int delay = 1;
	private float timer;

	public int height = 20;
	public int width = 20;
	public int number = 20;

	public override void Awake ()
	{
		base.Awake ();
		timer = delay;
	}

	// Update is called once per frame
	public override void Update () 
	{
		timer -= Time.deltaTime;
		if(timer <= 0)
		{
			for(int i = 0; i < number; i++)
			{
				PickupType pt;
				if(Random.value > 0.5)
				{
					pt = PickupType.Power;
				}
				else
				{
					pt = PickupType.Point;
				}
				GameObjectManager.Pickups.Spawn( new Vector3(2 * Random.value * width - width, 40 + 2 * Random.value * height - height), pt);
			}
			timer = delay;
		}
	}
}

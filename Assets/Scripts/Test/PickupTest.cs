using UnityEngine;
using System.Collections;
using DanmakuEngine.Core;

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
				Pickup.Type pt;
				pt = (Pickup.Type)Mathf.FloorToInt(Random.value * 4);
				PickupPool.Spawn( new Vector3(2 * Random.value * width - width, 40 + 2 * Random.value * height - height), pt).GameObject.SetActive(true);
			}
			timer = delay;
		}
	}
}

using UnityEngine;
using System.Collections;

public class PickupTest : MonoBehaviour 
{
	public int delay = 1;
	private float timer;

	public int height = 20;
	public int width = 20;
	public int number = 20;

	void Start()
	{
		timer = delay;
	}

	// Update is called once per frame
	void Update () 
	{
		timer -= Time.deltaTime;
		if(timer <= 0)
		{
			for(int i = 0; i < number; i++)
			{
				Pickup p = GameObjectManager.Pickups.Get();
				p.trans.position = new Vector3(2 * Random.value * width - width, 40 + 2 * Random.value * height - height);
				if(Random.value > 0.5)
				{
					p.type = PickupType.Power;
				}
				else
				{
					p.type = PickupType.Point;
				}
			}
			timer = delay;
		}
	}
}

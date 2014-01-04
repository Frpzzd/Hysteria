using UnityEngine;
using System.Collections;

public class Option : CachedObject
{
	private const float minAngle = -45;
	private const float maxAngle = 45;
	
	public void Fire()
	{
		if(Player.Sensing)
		{
			GameObjectManager.PlayerShots.Spawn (Transform.position, Quaternion.Euler(0,0, Random.Range(minAngle, maxAngle) / ((Player.Focused) ? 2 : 1)) , false);
		}
		else
		{
			GameObjectManager.PlayerShots.Spawn (Transform.position, false);
		}
	}
}

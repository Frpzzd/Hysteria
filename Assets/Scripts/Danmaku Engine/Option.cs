using UnityEngine;
using System.Collections;
using JamesLib;
using DanmakuEngine.Core;

public class Option : CachedObject
{
	private const float minAngle = -45;
	private const float maxAngle = 45;
	
	public PlayerShot Fire()
	{
		if(Player.Sensing)
		{
			return PlayerShotPool.Spawn (Transform.position, Quaternion.Euler(0,0, Random.Range(minAngle, maxAngle) / ((Player.Focused) ? 2 : 1)) , false);
		}
		else
		{
			return PlayerShotPool.Spawn (Transform.position, false);
		}
	}
}

using UnityEngine;
using System.Collections;
using JamesLib;
using DanmakuEngine.Core;

public class Option : CachedObject
{
	private float angle = 0f;

	public void Update()
	{
		float px, py, x, y;
		x = Transform.position.x;
		y = Transform.position.y;
		px = Player.PlayerTransform.position.x;
		py = Player.PlayerTransform.position.y;
		angle = Mathf.Atan2 (py - y, px - x) * (180f / Mathf.PI) - 90f;
	}
	
	public PlayerShot Fire()
	{
		if(Player.Sensing)
		{
			return PlayerShotPool.Spawn (Transform.position, Quaternion.Euler(0,0, angle / ((Player.Focused) ? 2 : 1)) , false);
		}
		else
		{
			return PlayerShotPool.Spawn (Transform.position, false);
		}
	}
}

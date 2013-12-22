using UnityEngine;
using System.Collections;

public class Option : CachedObject
{
	public float angle;
	private Quaternion rotation;
	private Quaternion focusedRotation;

	// Use this for initialization
	public override void Awake() 
	{
		base.Awake ();
		rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
		focusedRotation = Quaternion.Euler(new Vector3(0, 0, (angle - 90) / 2));
	}
	
	public void Fire()
	{
		if(Player.Sensing)
		{
			if(Player.Focused)
			{
				GameObjectManager.PlayerShots.Spawn (Transform.position, focusedRotation, false);
			}
			else
			{
				GameObjectManager.PlayerShots.Spawn (Transform.position, rotation, false);
			}
		}
		else
		{
			GameObjectManager.PlayerShots.Spawn (Transform.position, false);
		}
	}
}

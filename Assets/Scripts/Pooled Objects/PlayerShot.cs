using UnityEngine;
using System.Collections;

public class PlayerShot : GameObjectManager.PooledGameObject<PlayerShot, bool>
{
	public bool mainShot;
	public float mainSpeed;
	public float optionSpeed;
	[HideInInspector]
	public Enemy target;
	public Color mainColor;
	public Color optionColor;

	public int DamageValue
	{
		get 
		{
			if(mainShot)
			{
				return Player.MainShotDamage;
			}
			else
			{
				return Player.OptionShotDamage;
			}
		}
	}

	public override void Activate (bool param)
	{
		mainShot = param;
		if(!mainShot)
		{
			if(Player.Judging)
			{
				float closestDistance = float.MaxValue;
				for(int i = 0; i < Enemy.enemiesInPlay.Count; i++)
				{
					float currentDistance = (Enemy.enemiesInPlay[i].Transform.position - Player.PlayerTransform.position).sqrMagnitude;
					if(currentDistance < closestDistance)
					{
						target = Enemy.enemiesInPlay[i];
					}
				}
			}
			(renderer as SpriteRenderer).color = optionColor;
			name = "Option Shot";
		}
		else
		{
			target = null;
			(renderer as SpriteRenderer).color = mainColor;
			name = "Main Shot";
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 pos = Transform.position;
		pos += Transform.up * ((mainShot) ? mainSpeed : optionSpeed)* Time.deltaTime;
		Transform.position = pos;
		if(target != null && !target.Dead)
		{

		}
	}
}

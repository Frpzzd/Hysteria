using UnityEngine;
using System.Collections;

public class PlayerShot : PooledGameObject<bool>
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
				return Player.instance.MainShotDamage;
			}
			else
			{
				return (int)(((Player.instance.Extravert) ? 8f : 1f) * ((Player.instance.Sensing) ? 2f : 0.5f) * Player.instance.baseOptionShotDamage);
			}
		}
	}

	public override void Activate (bool param)
	{
		mainShot = param;
		if(!mainShot)
		{
			if(Player.instance.Judging)
			{
				float closestDistance = float.MaxValue;
				for(int i = 0; i < Enemy.enemiesInPlay.Count; i++)
				{
					float currentDistance = (Enemy.enemiesInPlay[i].trans.position - Player.playerTransform.position).sqrMagnitude;
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
		Vector3 pos = trans.position;
		pos += trans.up * mainSpeed * Time.deltaTime;
		trans.position = pos;
		if(target != null && !target.Dead)
		{

		}
	}
}

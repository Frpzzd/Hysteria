using UnityEngine;
using System.Collections.Generic;

public class PlayerShot : GameObjectManager.PooledGameObject<PlayerShot, bool>
{
	public bool mainShot;
	public float mainSpeed;
	public float optionSpeed;
	[HideInInspector]
	public Enemy target;
	public Color mainColor;
	public Color optionColor;
	public List<Enemy> enemiesHit;

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

	public override void Awake()
	{
		base.Awake ();
		enemiesHit = new List<Enemy> ();
	}

	public override void Activate (bool param)
	{
		mainShot = param;
		enemiesHit.Clear ();
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
	void FixedUpdate () 
	{
		//Raycast check for enemies
		float distance = ((mainShot) ? mainSpeed : optionSpeed) * Time.fixedDeltaTime;
		Debug.DrawRay (Transform.position, Transform.up * ((mainShot) ? mainSpeed : optionSpeed) * Time.deltaTime);
		RaycastHit2D raycastHit = Physics2D.Raycast(Transform.position.XY(), Transform.up.XY(), distance, 256); //256 = 2^8, Layer 8 is  Enemies
		if(raycastHit)
		{
			Enemy enemy = raycastHit.collider.gameObject.GetComponent<Enemy>();
			if(enemy != null)
			{
				if(enemiesHit.Contains(enemy))
				{
					Transform.position  += Transform.up * distance;
				}
				else
				{
					enemy.Damage(DamageValue);
					Transform.position = raycastHit.point.ToVector3();
					//TODO: Play Enemy hit effect here;
					if(!Player.Percieving || mainShot)
					{
						GameObjectManager.PlayerShots.Return(this);
					}
					else
					{
						enemiesHit.Add(enemy);
					}
				}
			}
			else
			{
				Debug.Log("Hit Something that isn't an enemy");
			}
		}
		else
		{
			Transform.position  += Transform.up * distance;
			
			//TODO: Complete Homing shot code
			if(target != null && !target.Dead)
			{
				
			}
		}
	}
}

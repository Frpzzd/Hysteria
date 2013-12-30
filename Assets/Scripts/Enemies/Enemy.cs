using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class EnemyDrops
{
	public int point;
	public int power;
	public bool life;
	public bool bomb;
}

public class Enemy : CachedObject, NamedObject
{
	public static List<Enemy> enemiesInPlay;
	public bool boss;
	public AttackPattern[] attackPatterns;
	public string enemyName;
	public float dropRadius;
	public string title;

	static Enemy()
	{
		enemiesInPlay = new List<Enemy> ();
	}

	public bool Dead
	{
		get { return enemiesInPlay.Contains(this); }
	}

	public string Name
	{
		get 
		{
			if(enemyName == null)
			{
				enemyName = name;
			}
			return enemyName;
		}

		set
		{
			enemyName = value;
		}
	}

	private int currentAttackPattern;

	public override void Awake()
	{
		foreach(Enemy e in GetComponentsInChildren<Enemy>(true))
		{
			if(e != this)
			{
				Destroy(e);
			}
		}
		List<AttackPattern> myAttackPatterns = new List<AttackPattern> (attackPatterns);
		AttackPattern[] allAttackPatterns = GetComponentsInChildren<AttackPattern> (true);
		for(int i = 0; i < allAttackPatterns.Length; i++)
		{
			if(!myAttackPatterns.Contains(allAttackPatterns[i]))
			{
				Destroy(allAttackPatterns[i]);
			}
			else
			{
				allAttackPatterns[i].enabled = false;
			}
		}
		currentAttackPattern = 0;
	}

	public void Spawn()
	{
		enemiesInPlay.Add (this);
		collider2D.enabled = true;
		renderer.enabled = true;
	}
	
	// Update is called once per frame
	void Update () 
	{

	}

	void Damage(int amount)
	{
		attackPatterns[currentAttackPattern].currentHealth -= amount;
		if(attackPatterns[currentAttackPattern].currentHealth < 0)
		{
			attackPatterns[currentAttackPattern].enabled = false;
			Drop (attackPatterns[currentAttackPattern].drops);
			currentAttackPattern++;
			if(currentAttackPattern >= attackPatterns.Length)
			{
				Die ();
			}
			else
			{
				attackPatterns[currentAttackPattern].enabled = true;
			}
		}
	}

	public void Die()
	{
		enemiesInPlay.Remove (this);
		//TO-DO: Play enemy death visual effect here
		//TO-DO: Play enemy death sound effect here
		Destroy (gameObject);
	}

	void Drop(EnemyDrops drop)
	{
		Vector3 pos = transform.position;
		float angle, distance;
		for(int i = 0; i < drop.point; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = dropRadius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Point);
		}
		for(int i = 0; i < drop.power; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = dropRadius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Power);
		}
		if(drop.life)
		{
			GameObjectManager.Pickups.Spawn(Transform.position, PickupType.Life);
		}
		if(drop.bomb)
		{
			GameObjectManager.Pickups.Spawn(Transform.position, PickupType.Bomb);
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if(col.gameObject.layer == 9) // Player Shots
		{
			PlayerShot shot = col.GetComponent<PlayerShot>();
			Damage (shot.DamageValue);
			if(!Player.Percieving || shot.mainShot)
			{
				GameObjectManager.PlayerShots.Return(shot);
			}
			//TO-DO: PLay enemy hit effect herer
		}
	}
}

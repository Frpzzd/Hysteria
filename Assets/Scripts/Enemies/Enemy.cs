using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class EnemyDrops
{
	public float radius;
	public int point;
	public int power;
	public int life;
	public int bomb;
}

public class Enemy : CachedObject
{
	public static List<Enemy> enemiesInPlay;
	public int health;
	private int currentHealth;
	public bool boss;
	private AttackPattern[] attackPatterns;

	static Enemy()
	{
		enemiesInPlay = new List<Enemy> ();
	}
	
	[NonSerialized]
	public Transform trans;

	public bool Dead
	{
		get { return enemiesInPlay.Contains(this); }
	}

	public EnemyDrops drops;
	private int currentAttackPattern;

	public override void Awake()
	{
		attackPatterns = GetComponents<AttackPattern> ();
		for(int i = 0; i < attackPatterns.Length; i++)
		{
			attackPatterns[i].enabled = false;
		}
		currentAttackPattern = 0;
		currentHealth = health;
	}

	public void Spawn()
	{
		enemiesInPlay.Add (this);
		collider2D.enabled = true;
		renderer.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Damage(int amount)
	{
		if(boss)
		{
			attackPatterns[currentAttackPattern].currentHealth -= amount;
			if(attackPatterns[currentAttackPattern].currentHealth < 0)
			{
				attackPatterns[currentAttackPattern].enabled = false;
				Drop (attackPatterns[currentAttackPattern].drops);
				currentAttackPattern++;
				if(currentAttackPattern > attackPatterns.Length)
				{
					Die ();
				}
				else
				{
					attackPatterns[currentAttackPattern].enabled = true;
				}
			}
		}
		else
		{
			currentHealth -= amount;
			if(currentHealth < 0)
			{
				Die();
			}
		}
	}

	void Die()
	{
		if(boss)
		{

		}
		else
		{
			Drop (drops);
		}
		enemiesInPlay.Remove (this);
		collider2D.enabled = false;
		renderer.enabled = false;
		//TO-DO: Play enemy death visual effect here
		//TO-DO: Play enemy death sound effect here
	}

	void Drop(EnemyDrops drop)
	{
		Vector3 pos = transform.position;
		float angle, distance;
		for(int i = 0; i < drop.point; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = drop.radius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Point);
		}
		for(int i = 0; i < drop.power; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = drop.radius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Power);
		}
		for(int i = 0; i < drop.life; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = drop.radius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Life);
		}
		for(int i = 0; i < drop.bomb; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = drop.radius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Bomb);
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

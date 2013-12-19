using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct EnemyDrops
{
	public float radius;
	public int point;
	public int power;
	public int life;
	public int bomb;
}

public class Enemy : MonoBehaviour 
{
	public static List<Enemy> enemiesInPlay;
	public int health;
	public int currentHealth;
	public bool boss;
	private AttackPattern[] attackPatterns;

	static Enemy()
	{
		enemiesInPlay = new List<Enemy> ();
	}

	[HideInInspector]
	public Transform trans;

	public bool Dead
	{
		get { return enemiesInPlay.Contains(this); }
	}

	public EnemyDrops drops;
	private int currentAttackPattern;

	void Awake()
	{
		attackPatterns = GetComponents<AttackPattern> ();
		for(int i = 0; i < attackPatterns.Length; i++)
		{
			attackPatterns[i].enabled = false;
		}
		currentAttackPattern = 0;
		trans = transform;
	}

	public void Spawn()
	{
		enemiesInPlay.Add (this);
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
		for(int i = 0; i < drop.point; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = drop.radius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), PickupType.Bomb);
		}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if(col.tag == "Player Shot")
		{
			PlayerShot shot = col.GetComponent<PlayerShot>();
			Damage (shot.DamageValue);
			if(!Player.instance.Percieving)
			{
				GameObjectManager.PlayerShots.Return(shot);
			}
			//TO-DO: PLay enemy hit effect herer
		}
	}
}

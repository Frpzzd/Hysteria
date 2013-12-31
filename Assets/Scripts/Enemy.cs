using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyDrops
{
	[SerializeField]
	public int point;
	[SerializeField]
	public int power;
	[SerializeField]
	public bool life;
	[SerializeField]
	public bool bomb;
}

[System.Serializable]
public class Enemy : CachedObject, NamedObject, TitledObject
{
	public static List<Enemy> enemiesInPlay;
	[SerializeField]
	public bool boss;
	[SerializeField]
	public AttackPattern[] attackPatterns;
	[SerializeField]
	public string enemyName;
	[SerializeField]
	public float dropRadius;
	[SerializeField]
	public string enemyTitle;

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

	public string Title
	{
		get 
		{
			if(enemyTitle == null)
			{
				enemyTitle = "";
			}
			return enemyTitle;
		}

		set
		{
			enemyTitle = value;
		}
	}

	private AttackPattern currentAttackPattern;

	public void Start()
	{
		StartCoroutine (RunAttackPatterns ());
	}
		
	public IEnumerator RunAttackPatterns()
	{
		foreach(AttackPattern pattern in attackPatterns)
		{
			currentAttackPattern = pattern;
			pattern.Initialize(this);
			yield return StartCoroutine(pattern.Run(this));
		}
		Die ();
	}

	void Damage(int amount)
	{
		if(currentAttackPattern != null)
		{
			currentAttackPattern.Damage(amount);
		}
	}

	public void Die()
	{
		enemiesInPlay.Remove (this);
		//TO-DO: Play enemy death visual effect here
		//TO-DO: Play enemy death sound effect here
		collider2D.enabled = false;
		renderer.enabled = false;
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

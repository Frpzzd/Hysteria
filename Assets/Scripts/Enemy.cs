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
[RequireComponent(typeof(Collider2D))]
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
			Drop (pattern.drops);
		}
		Die ();
	}

	public void Damage(int amount)
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
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), Pickup.Type.Point);
		}
		for(int i = 0; i < drop.power; i++)
		{
			angle = 2 * Mathf.PI * UnityEngine.Random.value;
			distance = dropRadius * UnityEngine.Random.value;
			GameObjectManager.Pickups.Spawn(new Vector3(pos.x + Mathf.Cos(angle) * distance, pos.y + Mathf.Sin(angle) * distance), Pickup.Type.Power);
		}
		if(drop.life)
		{
			GameObjectManager.Pickups.Spawn(Transform.position, Pickup.Type.Life);
		}
		if(drop.bomb)
		{
			GameObjectManager.Pickups.Spawn(Transform.position, Pickup.Type.Bomb);
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		DrawGizmos (Transform.position);
	}

	public void DrawGizmos(Vector3 spawnPosition)
	{
		Color oldColor = Gizmos.color;
		Gizmos.color = Color.yellow;
		Vector3 endLocation = spawnPosition;
		if(attackPatterns != null)
		{
			foreach(AttackPattern pattern in attackPatterns)
			{
				endLocation = pattern.DrawGizmos (endLocation, Color.yellow);
			}
		}
		Gizmos.color = oldColor;
	}
	#endif
	void OnTriggerEnter2D(Collider2D col)
	{
		Debug.Log ("Enemy Hit");
		if(col.gameObject.layer == 9) // Player Shots
		{
			PlayerShot shot = col.GetComponent<PlayerShot>();
			Damage (shot.DamageValue);
			//TODO: Play enemy hit effect herer
		}
	}
}

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
}

[System.Serializable]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
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
	public AudioClip bossTheme;
	public float startYPosition;

	static Enemy()
	{
		enemiesInPlay = new List<Enemy> ();
	}

	public bool Dead
	{
		get { return !enemiesInPlay.Contains(this); }
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

	[HideInInspector]
	public AttackPattern currentAttackPattern
	{
		get 
		{ 
			return (apSelect >= 0 && apSelect < attackPatterns.Length) ? attackPatterns [apSelect] : null;
		}
	}

	public int RemainingAttackPatterns
	{
		get { return attackPatterns.Length - apSelect; }
	}

	public int apSelect;

	public void Spawn()
	{
		enemiesInPlay.Add (this);
		StartCoroutine (RunAttackPatterns ());
	}
		
	public IEnumerator RunAttackPatterns()
	{
		for(apSelect = 0; apSelect < attackPatterns.Length; apSelect++)
		{
			currentAttackPattern.Initialize(this);
			yield return StartCoroutine(currentAttackPattern.Run(this));
			if(!boss || currentAttackPattern.success)
			{
				Drop (currentAttackPattern.drops);
			}
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
		StopAllCoroutines ();
		enemiesInPlay.Remove (this);
		//TO-DO: Play enemy death visual effect here
		//TO-DO: Play enemy death sound effect here
		collider2D.enabled = false;
		renderer.enabled = false;
	}

	public void TestAttackPattern(int ap)
	{
		apSelect = ap;
		StartCoroutine (TestRunAttackPattern ());
	}

	public IEnumerator TestRunAttackPattern()
	{
		float start = Time.time;
		currentAttackPattern.Initialize(this);
		yield return StartCoroutine(currentAttackPattern.Run(this));
		Drop (currentAttackPattern.drops);
		Die ();
		Debug.Log (Time.time - start + " total seconds.");
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
	}

	#if UNITY_EDITOR
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

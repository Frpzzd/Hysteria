using System.Collections;
using UnityEngine;
using JamesLib;

namespace DanmakuEngine.Core
{
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(SpriteRenderer))]
	[RequireComponent(typeof(AbstractAttackPattern))]
	public abstract class AbstractEnemy : CachedObject, NamedObject
	{
		public abstract string Name { get; set; }

		public abstract AbstractAttackPattern currentAttackPattern { get; }

		[System.NonSerialized]
		private bool dead;

		public bool Dead
		{
			get { return dead; }
		}

		[SerializeField]
		public float dropRadius;

		public void Spawn()
		{
			//TODO:FIX
			//enemiesInPlay.Add (this);
			dead = false;
			StartCoroutine (Run());
		}

		protected abstract IEnumerator Run();

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
			//TO-DO: Play enemy death visual effect here
			//TO-DO: Play enemy death sound effect here
			collider2D.enabled = false;
			renderer.enabled = false;
			dead = true;
		}

		void OnTriggerEnter2D(Collider2D col)
		{
			if(col.gameObject.layer == 9) // Player Shots
			{
				PlayerShot shot = col.GetComponent<PlayerShot>();
				Damage (shot.DamageValue);
				//TODO: Play enemy hit effect herer
			}
		}
	}
}


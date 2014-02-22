using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Actions;
using JamesLib;

namespace DanmakuEngine.Core
{
	[Serializable]
	public abstract class AbstractAttackPattern : CachedObject, NamedObject, TitledObject
	{
		[HideInInspector]
		[SerializeField]
		public AbstractEnemy parent;
		public DropHandler drops;
		
		[SerializeField]
		public string apName = "Attack Pattern";
		[SerializeField]
		public string apTitle = "Title";
		[SerializeField]
		public int health = 100;
		[SerializeField]
		public int timeout;
		[SerializeField]
		public int bonus;
		[SerializeField]
		public bool survival;
		
		[NonSerialized]
		public List<Bullet> bulletsInPlay;
		[NonSerialized]
		public int currentHealth;
		[NonSerialized]
		public int secondsRemaining;
		[NonSerialized]
		public int remainingBonus;
		[NonSerialized]
		public float remainingTime;
		[NonSerialized]
		public bool success;
		
		public string Name
		{
			get { return apName; }
			set { apName = value; }
		}
		
		public string Title
		{
			get 
			{ 
				if(apTitle == null)
				{
					apTitle = "";
				}
				return apTitle; 
			}
			set { apTitle = value; }
		}

		//TODO: Make Virtual instead
		public abstract IEnumerator Run ();
		
		public virtual void Initialize(AbstractEnemy parent)
		{
			this.parent = parent;
		}

		public abstract void DrawHandles (Vector3 spawnPosition, bool mirrorMove, Color handlesColor);
		
		public void Damage(int amount)
		{
			currentHealth -= amount;
		}

		protected void Fire(Bullet temp, Vector3 sourcePos, float angle, float speed, bool fake)
		{
			temp.Transform.position = sourcePos;
			temp.Transform.rotation = Quaternion.Euler(0,0, angle);
			temp.velocity.x = speed;
			temp.velocity.y = 0f;
			temp.master = this;
			temp.fake = fake;
			bulletsInPlay.Add (temp);
			temp.GameObject.SetActive (true);
		}
	}
}
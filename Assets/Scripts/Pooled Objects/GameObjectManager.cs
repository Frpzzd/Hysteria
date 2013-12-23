using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class PooledGameObject<P> : CachedObject
{
	public abstract void Activate(P param);

	public virtual void LateActivate()
	{
	}
}

public class GameObjectManager : StaticGameObject<GameObjectManager>
{
	[Serializable]
	public class GameObjectPool<T, P> : Queue<T> where T : PooledGameObject<P> , new()
	{
		public GameObject blankPrefab;
		public GameObject container;
		[NonSerialized]
		public List<T> All = new List<T>();
		public int Preallocation = 5;
		public int UponEmptySpawn = 1;
		private bool started = false;

		public void Start ()
		{
			if(!started)
			{
				for(int i = 0; i < Preallocation; i++)
				{
					Enqueue(CreateNew());
				}
				started = true;
			}
		}

		private T CreateNew()
		{
			GameObject go = (GameObject)Instantiate (blankPrefab);
			T newT = go.GetComponent<T>();
			go.SetActive (false);
			go.transform.parent = container.transform;
			All.Add (newT);
			return newT;
		}

		private T CustomDequeue()
		{
			if(Count == 0)
			{
				for(int i = 0; i < UponEmptySpawn; i++)
				{
					Enqueue(CreateNew());
				}
			}
			return Dequeue();
		}

		public T Get(P param)
		{
			T newT = CustomDequeue();
			newT.Activate(param);
			return newT;
		}

		public T Spawn(Vector3 pos, P param)
		{
			return Spawn (pos, Quaternion.identity, param);
		}

		public T Spawn(Vector3 pos, Quaternion rotation, P param)
		{
			T newT = Get(param);
			newT.Transform.position = pos;
			newT.Transform.rotation = rotation;
			newT.GameObject.SetActive (true);
			newT.LateActivate ();
			return newT;
		}

		public void Return(T t)
		{
			t.GameObject.SetActive (false);
			Enqueue(t);
		}
	}

	[Serializable]
	public class BulletPoolSubset : GameObjectPool<Bullet, BulletTag> { }

	[Serializable]
	public class BulletPool
	{
		public BulletPoolSubset circleBullets;
		public BulletPoolSubset boxBullets;

		public void Start()
		{
			circleBullets.Start ();
			boxBullets.Start ();
		}

		public void Return(Bullet bullet)
		{
			Type colliderType = bullet.col.GetType ();
			if(colliderType == typeof(CircleCollider2D))
			{
				circleBullets.Return(bullet);
			}
			if(colliderType == typeof(BoxCollider2D))
			{
				boxBullets.Return(bullet);
			}
		}

		public Bullet Get(BulletTag tag)
		{
			Bullet bullet = null;
			Collider2D col = tag.prefab.collider2D;
			Type colliderType = col.GetType ();
			if(colliderType == typeof(CircleCollider2D))
			{
				bullet = circleBullets.Get (tag);
				CircleCollider2D bulletCol = bullet.col as CircleCollider2D;
				CircleCollider2D circleCol = col as CircleCollider2D;
				bulletCol.center = circleCol.center;
				bulletCol.radius = circleCol.radius;
				return bullet;
			}
			if(colliderType == typeof(BoxCollider2D))
			{
				bullet = circleBullets.Get (tag);
				BoxCollider2D bulletCol = bullet.col as BoxCollider2D;
				BoxCollider2D circleCol = col as BoxCollider2D;
				bulletCol.center = circleCol.center;
				bulletCol.size = circleCol.size;
				return bullet;
			}
			return null;
		}

	}
	[Serializable]
	public class PickupPool : GameObjectPool<Pickup, PickupType> { }
	[Serializable]
	public class PlayerShotPool : GameObjectPool<PlayerShot, bool> { }
	[Serializable]
	public class ScorePopupPool : GameObjectPool<ScorePopup, ScorePopup.Params> { }

	public BulletPool bullets;
	public PickupPool pickups;
	public PlayerShotPool playerShots;
	public ScorePopupPool scorePopups;
	
	public static BulletPool Bullets
	{
		get { return instance.bullets; }
	}
	
	public static PickupPool Pickups
	{
		get { return instance.pickups; }
	}

	public static PlayerShotPool PlayerShots
	{
		get { return instance.playerShots; }
	}

	public static ScorePopupPool ScorePopups
	{
		get { return instance.scorePopups; }
	}

	public override void Awake()
	{
		base.Awake ();
		bullets.Start ();
		pickups.Start ();
		playerShots.Start ();
		scorePopups.Start ();
	}
}

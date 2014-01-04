using UnityEngine;
using System.Collections.Generic;
using System;

public class GameObjectManager : StaticGameObject<GameObjectManager>
{
	[Serializable]
	public class GameObjectPool<T, P> : Stack<T> where T : PooledGameObject<T, P>
	{
		public GameObject blankPrefab;
		[NonSerialized]
		public List<T> All = new List<T>();
		public int Preallocation = 5;
		public int UponEmptySpawn = 1;
		private bool started = false;

		public int TotalActive
		{
			get { return All.Count - Count; }
		}

		public void Start ()
		{
			if(!started)
			{
				for(int i = 0; i < Preallocation; i++)
				{
					Push(CreateNew());
				}
				started = true;
			}
		}

		private T CreateNew()
		{
			GameObject go = (GameObject)Instantiate (blankPrefab);
			go.hideFlags = HideFlags.HideInHierarchy;
			DontDestroyOnLoad (go);
			T newT = go.GetComponent<T>();
			go.SetActive (false);
			All.Add (newT);
			return newT;
		}

		private T CustomDequeue()
		{
			if(Count == 0)
			{
				for(int i = 0; i < UponEmptySpawn; i++)
				{
					Push(CreateNew());
				}
			}
			return Pop();
		}

		public virtual T Get(P param)
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
			Push(t);
		}
	}

	public abstract class PooledGameObject<T, P> : CachedObject where T : PooledGameObject<T, P>
	{
		public abstract void Activate(P param);
		
		public virtual void LateActivate()
		{
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
	public class PickupPool : GameObjectPool<Pickup, Pickup.Type> 
	{
		public Sprite pointSprite;
		public Sprite powerSprite;
		public Sprite bombSprite;
		public Sprite lifeSprite;
		public Sprite pointValueSprite;
	
		public Color pointColor;
		public Color powerColor;
		public Color bombColor;
		public Color lifeColor;
		public Color pointValueColor;

		public override Pickup Get (Pickup.Type param)
		{
			Pickup p = base.Get (param);
			switch(param)
			{
				case Pickup.Type.Point:
					p.render.sprite = pointSprite;
					p.render.color = pointColor;
					break;
				case Pickup.Type.Power:
					p.render.sprite = powerSprite;
					p.render.color = powerColor;
					break;
				case Pickup.Type.Bomb:
					p.render.sprite = bombSprite;
					p.render.color = bombColor;
					break;
				case Pickup.Type.Life:
					p.render.sprite = lifeSprite;
					p.render.color = lifeColor;
					break;
				case Pickup.Type.PointValue:
					p.render.sprite = pointValueSprite;
					p.render.color = pointValueColor;
					break;
			}
			return p;
		}

		public void AutoCollectAll()
		{
			foreach(Pickup pickup in All)
			{
				pickup.state = Pickup.State.AutoCollect;
				pickup.currentVelocity = -1f;
			}
		}
	}
	[Serializable]
	public class PlayerShotPool : GameObjectPool<PlayerShot, bool> { }

	public BulletPool bullets;
	public PickupPool pickups;
	public PlayerShotPool playerShots;
	
	public static BulletPool Bullets
	{
		get { return Instance.bullets; }
	}
	
	public static PickupPool Pickups
	{
		get { return Instance.pickups; }
	}

	public static PlayerShotPool PlayerShots
	{
			get { return Instance.playerShots; }
	}

	public override void Awake()
	{
		base.Awake ();
		bullets.Start ();
		pickups.Start ();
		playerShots.Start ();
	}
}

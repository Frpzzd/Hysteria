using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class PooledGameObject : MonoBehaviour
{
	[HideInInspector]
	public Transform trans;
	[HideInInspector]
	public GameObject gameObj;
	[HideInInspector]
	public Rigidbody2D rigBody;

	public abstract void Activate();
}

public class GameObjectManager : MonoBehaviour 
{
	[Serializable]
	public class GameObjectPool<T> : Queue<T> where T : PooledGameObject
	{
		public GameObject blankPrefab;
		public int Preallocation = 5;
		public int UponEmptySpawn = 1;
		public float SpawnTime;
		public float Timer;

		private T CreateNew()
		{
			T newT =((GameObject)Instantiate (blankPrefab)).GetComponent<T>();
			newT.gameObj.SetActive (false);
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

		public T Get()
		{
			T newT = CustomDequeue();
			newT.Activate ();
			newT.gameObj.SetActive (true);
			return newT;
		}

		public void Return(T t)
		{
			t.gameObj.SetActive (false);
			Enqueue(t);
		}
	}

	[Serializable]
	public class BulletPool : GameObjectPool<Bullet> { }
	[Serializable]
	public class PickupPool : GameObjectPool<Pickup> { }

	public BulletPool bullets;
	public PickupPool pickups;

	public static BulletPool Bullets
	{
		get { return manager.bullets; }
	}

	public static PickupPool Pickups
	{
		get { return manager.pickups; }
	}

	public static GameObjectManager manager;

	void Start()
	{
		bullets = new BulletPool ();
		pickups = new PickupPool ();
		manager = this;
	}
}

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
		public int InitialSize;
		public bool Spawning;
		public int SpawnTarget;
		public float SpawnTime;
		public float Timer;

		private T CreateNew()
		{
			return default(T);
		}

		public T Get()
		{
			T newT = (Count != 0) ? Dequeue () : CreateNew ();
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

	static GameObjectManager()
	{
		Bullets = new BulletPool();
		Pickups = new PickupPool();
	}

	[Serializable]
	public class BulletPool : GameObjectPool<Bullet> { }
	[Serializable]
	public class PickupPool : GameObjectPool<Pickup> { }

	public static BulletPool Bullets;
	public static PickupPool Pickups;

	public static GameObject instance;
	public static GameObjectManager manager;
}

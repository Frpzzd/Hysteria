using UnityEngine;
using System.Collections.Generic;

public abstract class PooledGameObject : MonoBehaviour
{
	public Transform trans;
	public GameObject gameObj;
	public Rigidbody2D rigBody;

	public void Activate();
}

public class GameObjectManager : MonoBehaviour 
{	
	public class GameObjectPool<T> : Queue<T> where T : PooledGameObject
	{
		public GameObject blankPrefab;
		public int InitialSize;
		public bool Spawning;
		public int SpawnTarget;
		public float SpawnTime;
		public float Timer;

		private T CreateNew()

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
	public static GameObjectPool<Bullet> Bullets;
	public static GameObjectPool<Pickup> Pickups;

	public static GameObject instance;
	public static GameObjectManager manager;

	// Use this for initialization
	void Start () 
	{ 
		bulletPool = new Queue<Bullet>();
		instance = gameObject;
		manager = this;
		for(int i = 0; i < initialSize; i++)
		{
			ReturnBullet(NewBullet());
		}
	}

	void Update()
	{
		if(spawningBullets)
		{
			timer -= Time.deltaTime;
			if(timer < 0)
			{
				timer = spawnTime;
				ReturnBullet(NewBullet());
			}
			spawningBullets = bulletPool.Count < spawnTarget;
		}
	}

	public static void RequestBullets(int i, float expectedTime)
	{
		if(i > bulletPool.Count)
		{
			spawningBullets = true;
			spawnTime = timer = expectedTime / (i - bulletPool.Count);
		}
	}

	private static Bullet NewBullet()
	{
		Bullet newBullet = ((GameObject)Instantiate(manager.blankPrefab)).GetComponent<Bullet>();
		newBullet.transform.parent = instance.transform;
		return newBullet;
	}
}

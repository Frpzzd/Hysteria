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

	public virtual void Awake()
	{
		trans = transform;
		gameObj = gameObject;
		rigBody = rigidbody2D;
	}

	public abstract void Activate();
}

public class GameObjectManager : MonoBehaviour 
{
	[Serializable]
	public class GameObjectPool<T> : Queue<T> where T : PooledGameObject
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
			newT.gameObj = go;
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
	[Serializable]
	public class PlayerShotPool : GameObjectPool<PlayerShot> { }

	public BulletPool bullets;
	public PickupPool pickups;
	public PlayerShotPool mainPlayerShots;
	public PlayerShotPool optionPlayerShots;
	
	public static BulletPool Bullets
	{
		get { return manager.bullets; }
	}
	
	public static PickupPool Pickups
	{
		get { return manager.pickups; }
	}

	public static PlayerShotPool MainPlayerShots
	{
		get { return manager.mainPlayerShots; }
	}
	
	public static PlayerShotPool OptionPlayerShots
	{
		get { return manager.optionPlayerShots; }
	}

	public static GameObjectManager manager;

	void Awake()
	{
		bullets.Start ();
		pickups.Start ();
		mainPlayerShots.Start ();
		optionPlayerShots.Start ();
		manager = this;
	}
}

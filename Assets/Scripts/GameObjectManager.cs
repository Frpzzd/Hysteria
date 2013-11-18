using UnityEngine;
using System.Collections.Generic;

public abstract class PooledGameObject : MonoBehaviour
{
	public Transform trans;
	public GameObject gameObj;
	public Rigidbody2D rigBody;

	public abstract void Activate();
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
	public static GameObjectPool<Bullet> Bullets;
	public static GameObjectPool<Pickup> Pickups;

	public static GameObject instance;
	public static GameObjectManager manager;
}

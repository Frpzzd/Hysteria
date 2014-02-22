using UnityEngine;
using System.Collections.Generic;
using JamesLib;

namespace DanmakuEngine.Core
{
	public abstract class PooledGameObject<T, P> : CachedObject where T : PooledGameObject<T, P> where P : GameObjectPool<T, P>
	{
		public virtual void Activate(object[] param)
		{
		}
	}

	[System.Serializable]
	public abstract class GameObjectPool<T, P> : StaticGameObject<GameObjectPool<T, P>> where T : PooledGameObject<T, P> where P : GameObjectPool<T, P>
	{
		public GameObject blankPrefab;
		[System.NonSerialized]
		private static Stack<T> Free = new Stack<T>();
		public static List<T> All = new List<T>();
		public int Preallocation = 5;
		public int UponEmptySpawn = 1;
		private bool started = false;

		public static int TotalActive
		{
			get { return All.Count - Free.Count; }
		}

		void Start ()
		{
			if(!started)
			{
				for(int i = 0; i < Instance.Preallocation; i++)
				{
					Free.Push(CreateNew());
				}
				started = true;
			}
		}
		
		private static T CreateNew()
		{
			GameObject go = (GameObject)Instantiate (Instance.blankPrefab);
			go.hideFlags = HideFlags.HideInHierarchy;
			DontDestroyOnLoad (go);
			T newT = go.GetComponent<T>();
			go.SetActive (false);
			All.Add (newT);
			return newT;
		}

		public static T Get()
		{
			if(Free.Count == 0)
			{
				for(int i = 0; i < Instance.UponEmptySpawn; i++)
				{
					Free.Push(CreateNew());
				}
			}
			return Free.Pop();
		}
		
		public static T Get(params object[] param)
		{
			T instance = Get ();
			instance.Activate (param);
			return instance;
		}

		public static T Spawn(Vector3 location, params object[] param)
		{
			T instance = Get ();
			instance.Transform.position = location;
			instance.Transform.rotation = Quaternion.identity;
			instance.Activate (param);
			return instance;
		}
		
		public static T Spawn(Vector3 location, Quaternion rotation, params object[] param)
		{
			T instance = Get ();
			instance.Transform.position = location;
			instance.Transform.rotation = rotation;
			instance.Activate (param);
			return instance;
		}
		
		public static void Return(T t)
		{
			t.GameObject.SetActive (false);
			Free.Push(t);
		}
	}
}
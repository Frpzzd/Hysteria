using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class StaticGameObject<T> : CachedObject where T : StaticGameObject<T>
{
	protected static T instance;

	public static T Instance
	{
		get { return instance; }
	}

	public bool keepBetweenScenes;
	public bool destroyNewInstances;

	public override void Awake ()
	{
		base.Awake ();
		if(instance != null)
		{
			if(instance.destroyNewInstances)
			{
				Destroy (gameObject);
				return;
			}
			else
			{
				Destroy (instance.GameObject);
			}
		}

		Debug.Log("Initializing " + typeof(T));

		instance = (T)this;
	
		if(keepBetweenScenes)
		{
			DontDestroyOnLoad (gameObject);
		}
	}
}
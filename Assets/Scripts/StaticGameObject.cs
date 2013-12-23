using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class StaticGameObject<T> : CachedObject where T : StaticGameObject<T>
{
	protected static T instance;

	public bool keepBetweenScenes;

	public override void Awake ()
	{
		base.Awake ();

		if(instance != null)
		{
			Debug.Log(instance.GetType().ToString() + " " + GetType());
			Destroy (gameObject);
			return;
		}

		Debug.Log ("Hello " + GetType());
		instance = (T)this;
	
		if(keepBetweenScenes)
		{
			DontDestroyOnLoad (gameObject);
		}
	}
}


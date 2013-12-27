using UnityEngine;
using System;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour 
{
	private static List<Timer> timers;

	private TimerManager instance;

	static TimerManager()
	{
		timers = new List<Timer> ();
	}

	public static void RegisterTimer(Timer t)
	{
		if(!timers.Contains(t))
		{
			timers.Add(t);
		}
	}

	void Awake()
	{
		if(instance != null && this != instance)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	void FixedUpdate()
	{
		float deltat = Time.fixedDeltaTime;
		foreach(Timer t in timers)
		{
			if(t.IsActive && !t.Done)
			{
				t.remainingTime -= deltat;
			}
		}
	}
}

[Serializable]
public class Timer
{
	[NonSerialized]
	public float remainingTime;
	public float totalTime;
	[NonSerialized]
	private bool active = false;

	public Timer()
	{
		TimerManager.RegisterTimer (this);
		this.remainingTime = totalTime;
	}

	public bool IsActive
	{
		get { return active; }
	}

	public bool Done
	{
		get { return remainingTime <= 0f; }
	}

	public void Start()
	{
		active = true;
	}

	public void Pause()
	{
		active = false;
	}

	public void Reset()
	{
		remainingTime = totalTime;
	}
}
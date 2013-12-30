using UnityEngine;
using System.Collections;

public abstract class TestScript : MonoBehaviour 
{
	#if UNITY_EDITOR
	public virtual void Awake()
	{
	}
	public virtual void Update()
	{
	}
	#else
	public virtual void Awake()
	{
		Destroy(this);
	}
	public virtual void Start()
	{
		Destroy (this);
	}
	public virtual void Update()
	{
		Destroy(this);
	}
	#endif
}

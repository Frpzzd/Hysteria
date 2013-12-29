using UnityEngine;
using System.Collections;

public abstract class TestScript : MonoBehaviour 
{
	#if UNITY_EDITOR
	public abstract void Awake();
	public virtual void Update()
	{
	}
	#else
	void Start()
	{
		Destroy (this);
	}
	#endif
}

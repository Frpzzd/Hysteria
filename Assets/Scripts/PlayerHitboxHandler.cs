using UnityEngine;
using System.Collections;

public class PlayerHitboxHandler : MonoBehaviour 
{
	[HideInInspector]
	private Transform  trans;
	[HideInInspector]
	public Renderer hitboxRenderer;
	public float rotationSpeed = 180;

	void Start()
	{
		hitboxRenderer = renderer;
		trans = transform;
	}

	void Update()
	{
		trans.Rotate(0,0,rotationSpeed * Time.deltaTime);
	}

	void OnTrigger(Collider col)
	{
	}
}

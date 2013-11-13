using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour 
{
	[HideInInspector]
	public Transform bulletTransform;
	[HideInInspector]
	public GameObject bulletObject;

	public BulletPattern master;

	public float speed;
	public bool grazed;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public class BulletPattern
{
	public List<Bullet> bulletList;
}
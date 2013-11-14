using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour 
{
	[HideInInspector]
	public Transform bulletTransform;
	[HideInInspector]
	public GameObject bulletObject;
	[HideInInspector]
	public Collider2D bulletCollider;

	public BulletPattern master;
	public BulletAction[] actions;

	public float speed;
	public bool grazed;

	// Use this for initialization
	void Start () 
	{
		gameObject.tag = "Enemy Bullet";
		bulletTransform = transform;
		bulletObject = gameObject;
		bulletCollider = collider2D;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void Cancel()
	{
		//Spawn Point Value at current location
		BulletManager.ReturnBullet(this);
	}
}

public class BulletAction
{
	public BulletActionType type = BulletActionType.Wait;
	public bool waitForChange = false;
}

public enum BulletActionType { Wait, Change_Direction, Start_Repeat, End_Repeat, Fire, Vertical_Change_Speed, Deactivate }
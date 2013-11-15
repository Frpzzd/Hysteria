using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour 
{	
	public GameObject blankPrefab;
	public static Queue<Bullet> bulletPool;
	public int initialSize;

	public static GameObject instance;
	public static BulletManager manager;
	public static bool spawningBullets;
	public static int spawnTarget;
	public static float spawnTime;
	public static float timer;

	// Use this for initialization
	void Start () 
	{ 
		bulletPool = new Queue<Bullet>();
		instance = gameObject;
		manager = this;
		for(int i = 0; i < initialSize; i++)
		{
			ReturnBullet(NewBullet());
		}
	}

	void Update()
	{
		if(spawningBullets)
		{
			timer -= Time.deltaTime;
			if(timer < 0)
			{
				timer = spawnTime;
				ReturnBullet(NewBullet());
			}
			spawningBullets = bulletPool.Count < spawnTarget;
		}
	}

	public static void RequestBullets(int i, float expectedTime)
	{
		if(i > bulletPool.Count)
		{
			spawningBullets = true;
			spawnTime = timer = expectedTime / (i - bulletPool.Count);
		}
	}

	private static Bullet NewBullet()
	{
		Bullet newBullet = ((GameObject)Instantiate(manager.blankPrefab)).GetComponent<Bullet>();
		newBullet.transform.parent = instance.transform;
		return newBullet;
	}

	public static Bullet GetBullet()
	{
		Bullet bullet = (bulletPool.Count != 0) ? bulletPool.Pop() : NewBullet();
		bullet.bulletObject.SetActive(true);
		return bullet;
	}

	public static void ReturnBullet(Bullet b)
	{
		b.bulletObject.SetActive(false);
		bulletPool.Push (b);
	}
}

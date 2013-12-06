using UnityEngine;
using System;
using System.Collections;

[Serializable]
public struct EnemyDrops
{
	public float radius;
	public int point;
	public int power;
	public int life;
	public int bomb;
}

public class Enemy : MonoBehaviour 
{
	public float health;
	public float currentHealth;

	// Use this for initialization
	void Start () {
	
	} 
	
	// Update is called once per frame
	void Update () {
	
	}
}

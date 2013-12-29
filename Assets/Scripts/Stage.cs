using UnityEngine;
using System;
using System.Collections;

public class Stage : MonoBehaviour 
{
	public int nextStageSceneNumber;
	public AudioClip stageMusic;
	public AudioClip bossMusic;

	public StageAction[] preboss;
	public StageAction boss;

	[Serializable]
	public class StageAction
	{
		public float WaitTime;
		public GameObject EnemyToSpawn;
		public Vector2 spawnLocation;
	}

	[NonSerialized]
	public Enemy[] currentInstances;
	[NonSerialized]
	public Enemy bossInstance;

	public void Awake()
	{
		currentInstances = new Enemy[preboss.Length];
		for(int i = 0; i < preboss.Length; i++)
		{
			if(preboss[i].EnemyToSpawn != null)
			{
				currentInstances[i] = ((GameObject)Instantiate(preboss[i].EnemyToSpawn)).GetComponent<Enemy>();
			}
			currentInstances[i].GameObject.SetActive(false);
			currentInstances[i].Transform.position = new Vector3(preboss[i].spawnLocation.x, preboss[i].spawnLocation.y, 0f);
		}
		if(boss.EnemyToSpawn != null)
		{
			bossInstance = ((GameObject)Instantiate (boss.EnemyToSpawn)).GetComponent<Enemy>();
			bossInstance.GameObject.SetActive (false);
			bossInstance.Transform.position = new Vector3 (boss.spawnLocation.x, boss.spawnLocation.y, 0f);
		}
	}

	public IEnumerator ExecuteStage()
	{
		for(int i = 0; i < preboss.Length; i++)
		{
			yield return new WaitForSeconds(preboss[i].WaitTime);
			currentInstances[i].GameObject.SetActive(true);
		}
		yield return new WaitForSeconds (boss.WaitTime);
		for(int i = 0; i < preboss.Length; i++)
		{
			currentInstances[i].Die();
		}
		if(bossInstance != null)
		{
			bossInstance.GameObject.SetActive (true);
		}
	}

	public void Cleanup()
	{

	}
}

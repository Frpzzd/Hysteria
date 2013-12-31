using UnityEngine;
using System.Collections;

public class TestPlayerSpawner : TestScript
{
	#if UNITY_EDITOR
	public GameObject playerPrefab;

	public override void Awake()
	{
		if(Player.Instance == null)
		{
			Instantiate (playerPrefab);
			Player.respawnLocation = GameObject.Find ("Player Respawn Location").transform;
		}
		Destroy (this);
	}
	#endif 
}

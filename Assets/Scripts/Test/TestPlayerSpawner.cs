using UnityEngine;
using System.Collections;

public class TestPlayerSpawner : TestScript
{
	#if UNITY_EDITOR
	public GameObject playerPrefab;
	public bool invincible;

	public override void Awake()
	{
		if(Player.Instance == null)
		{
			Player instance = (Player)((GameObject)Instantiate (playerPrefab)).GetComponent<Player>();
			instance.DebugInvincibility(invincible);
			Player.respawnLocation = GameObject.Find ("Player Respawn Location").transform;
		}
		Destroy (this);
	}
	#endif 
}

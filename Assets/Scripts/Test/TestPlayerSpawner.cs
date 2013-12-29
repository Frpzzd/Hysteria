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
		}
		Debug.Log ("Hello");
		Destroy (this);
	}
	#endif 
}

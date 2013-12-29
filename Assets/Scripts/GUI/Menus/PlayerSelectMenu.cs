using UnityEngine;
using System.Collections;

public class PlayerSelectMenu : Menu 
{
	public GameObject PlayerPrefab;

	private Player playerInEditing;

	public override void OnMenuEnter ()
	{
		playerInEditing = ((GameObject)Instantiate (PlayerPrefab)).GetComponent<Player> ();
		playerInEditing.GameObject.SetActive (false);
	}

	protected override void OnChildSwitchImpl (int i)
	{
		if(i == 4)
		{
			playerInEditing.GameObject.SetActive(true);
			StageManager.StartGame();
		}
	}
}

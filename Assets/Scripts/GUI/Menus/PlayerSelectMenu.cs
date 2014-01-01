using UnityEngine;
using System.Collections;

public class PlayerSelectMenu : Menu 
{
	public GameObject PlayerPrefab;

	private Player playerInEditing;

	public GUIText label;
	public GUIText descriptionArea;

	[Multiline]
	public string[] descriptions;
	
	private static string[] names = new string[] { "Extroverted\n", "Introverted\n", "Sensing\n", "Intuitive\n", "Thinking\n", "Feeling\n", "Judging\n", "Preceiving\n" };

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

	public override void Update()
	{
		base.Update ();
		if(playerInEditing != null)
		{
			playerInEditing.EI = FromMenu(0);
			playerInEditing.SN = FromMenu(1);
			playerInEditing.TF = FromMenu(2);
			playerInEditing.JP = FromMenu(3);
		}
		label.text = Player.ShotType;
		string descriptionString = "";
		for(int i = 0; i < 4; i++)
		{
			int index = 2 * i + children[i].selection;
			descriptionString += names[index];
			descriptionString += descriptions[index];
			descriptionString += "\n";	
		}
		descriptionArea.text = descriptionString;
	}

	private bool FromMenu(int index)
	{
		return children [index].selection == 0;
	}
}

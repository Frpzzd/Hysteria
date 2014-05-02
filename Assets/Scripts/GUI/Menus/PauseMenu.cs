using UnityEngine;
using System;
using System.Collections;

public class PauseMenu : InGameMenu
{
	public override void OnChildSwitch (int i)
	{
		switch(i)
		{
			case 0:
				Global.GameStateChange(GameState.InGame);
				break;
			case 1:
				StageManager.StartGame();
				break;
			case 2:
				Application.LoadLevel("Main Menu");
				break;
		}
	}
}

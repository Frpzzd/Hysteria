using UnityEngine;
using System.Collections;

public class GameOverMenu : InGameMenu
{
	public int ContinueIndex;
	public int RestartIndex;
	public int ReturnIndex;

	public override void OnChildSwitch (int i)
	{
		if(i == ContinueIndex)
		{
			Global.GameStateChange(GameState.Continue);
		}
		else if(i == RestartIndex)
		{
			StageManager.StartGame();
		}
		else if(i == ReturnIndex)
		{
			Application.LoadLevel("Main Menu");
		}
	}
}

using UnityEngine;
using System.Collections;

public class MainMenu : Menu
{
	public override bool ReturnToPrevious ()
	{
		bool success = selectedIndex != children.Length - 1;
		selectedIndex = children.Length - 1;
		return success;
	}

	protected override void OnChildSwitchImpl (int i)
	{
		switch(i)
		{
			case 0:			//Start Normal Game
				Global.GameType = GameType.Normal;
				StageManager.StartingStage = 1;
				break;
			case 5:
				Application.Quit();
				break;
		}
	}
}

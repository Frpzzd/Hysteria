using UnityEngine;
using System.Collections;

public class MainMenu : Menu
{
	protected override void OnParentSwitchImpl ()
	{
		Application.Quit ();
	}

	public override void ReturnToParent ()
	{
		Debug.Log ("hello");
		selectedIndex = buttonNames.Length - 1;
	}

	protected override void OnChildSwitchImpl (int i)
	{
		switch(i)
		{
			case 0:			//Start Normal Game
				Global.GameType = GameType.Normal;
				StageManager.SetStartLevel(1);
				break;
		}
	}
}

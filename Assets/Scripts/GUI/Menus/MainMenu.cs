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
}

using UnityEngine;
using System;
using System.Collections;

public class DifficultySelectMenu : Menu
{
	protected override void OnChildSwitchImpl (int i)
	{
		Global.Rank = (Rank)Enum.Parse(typeof(Rank), children[i].text.text);
	}
}

using UnityEngine;
using System.Collections;

public class PracticeStageSelectMenu : Menu 
{
	protected override void OnChildSwitchImpl (int i)
	{
		StageManager.StartingStage = i + 1;
	}
}

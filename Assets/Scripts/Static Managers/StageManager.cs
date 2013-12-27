using UnityEngine;
using System.Collections;

public class StageManager : StaticGameObject<StageManager> 
{
	public Stage[] stages;
	public static int currentStage;
	private static bool stageRunning;

	public static void NextStage()
	{
		stageRunning = false;
	}

	void FixedUpdate()
	{
		if(!stageRunning)
		{
			switch(Global.GameType)
			{
				case GameType.Normal:
					currentStage++;
					if(currentStage >= stages.Length)
					{
						//The Player wins the entire game
					}
					stages[currentStage].Initialize();
					StartCoroutine(stages[currentStage].ExecuteStage());
					break;
				case GameType.StagePractice:
				case GameType.AttackPractice:
					break;
			}
		}
	}
}

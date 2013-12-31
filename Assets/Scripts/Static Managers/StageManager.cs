using UnityEngine;
using System;
using System.Collections;

public class StageManager : StaticGameObject<StageManager> 
{
	public int[] stageSceneNumbers;
	public static Stage currentStage;
	private static int startingStage;

	public static void EndStage()
	{
		//TODO Show end of stage summary before starting next stage
		Debug.Log ("End Stage");
	}

	public static void NextStage()
	{
		Debug.Log ("Next Stage");
		switch(Global.GameType)
		{
			case GameType.Normal:
				int nextLevel = currentStage.nextStageSceneNumber;
				currentStage = null;
				Application.LoadLevel(nextLevel);
				break;
			case GameType.StagePractice:
			case GameType.AttackPractice:
				break;
		}
	}

	public static void SetStartLevel(int startStage)
	{
		startingStage = startStage - 1;
	}

	public static void StartGame()
	{
		LoadStage (startingStage);
	}

	public static void LoadStage(int stage)
	{
		currentStage = null;
		Application.LoadLevel (Instance.stageSceneNumbers [stage]);
	}
}

using UnityEngine;
using System;
using System.Collections;
using DanmakuEngine.Core;

public class StageManager : StaticGameObject<StageManager> 
{
	public GUIText label;
	public GUIText bonus;
	public float postStageWait;
	public float postBossSlowdown;

	private static int startingScene;
	public static int StartingStage
	{
		get { return startingScene - 1; }	
		set { startingScene = value + 1; }
	}
	private static int currentStage;

	public static IEnumerator EndStage(int bonus)
	{
		Debug.Log ("End Stage");
		PickupPool.AutoCollectAll ();
		float cachedTimeScale = Time.timeScale;
		Time.timeScale = Time.timeScale / Instance.postBossSlowdown;
		IEnumerator pause;
		float totalTime = 0;
		while(totalTime <= 1f)
		{
			pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			yield return new WaitForFixedUpdate();
			totalTime += Time.deltaTime;
		}
		Time.timeScale = cachedTimeScale;
		while(PickupPool.TotalActive > 0)
		{
			pause = Global.WaitForUnpause();
			while(pause.MoveNext())
			{
				yield return pause.Current;
			}
			yield return new WaitForFixedUpdate();
		}
		Instance.bonus.text = bonus.ToString ("N0");
		Instance.label.enabled = true;
		Instance.bonus.enabled = true;
		yield return new WaitForSeconds (Instance.postStageWait);
		Instance.label.enabled = true;
		Instance.bonus.enabled = true;
		NextStage ();
	}

	public static void NextStage()
	{
		Debug.Log ("Next Stage" + Global.GameType);
		switch(Global.GameType)
		{
			case GameType.Normal:
				if(currentStage > Application.levelCount - 2)
				{
					//Player has reached end of game, Player wins
					//Load ending
				}
				else
				{
					currentStage++;	
					LoadStage(currentStage);
				}
				break;
			case GameType.StagePractice:
				//Stage Practice has finished
				
			case GameType.AttackPractice:
				break;
		}
	}

	public static void StartGame()
	{
		Debug.Log ("Starting Game");
		Application.LoadLevel ("Main Gameplay");
		LoadStage (startingScene);
		currentStage = startingScene;
	}

	public static void LoadStage(int stage)
	{
		Debug.Log ("Loading Stage: " + (stage - 1));
		currentStage = stage;
		Application.LoadLevelAdditive (stage);
	}
}

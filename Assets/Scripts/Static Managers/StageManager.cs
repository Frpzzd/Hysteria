using UnityEngine;
using System;
using System.Collections;

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
		GameObjectManager.Pickups.AutoCollectAll ();
		float cachedTimeScale = Time.timeScale;
		Time.timeScale = Time.timeScale / Instance.postBossSlowdown;
		while(GameObjectManager.Pickups.TotalActive > 0)
		{
			yield return Instance.StartCoroutine(Global.WaitForUnpause());
			Debug.Log(GameObjectManager.Pickups.TotalActive);
			yield return new WaitForFixedUpdate();
		}
		Time.timeScale = cachedTimeScale;
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
		Debug.Log ("Next Stage");
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
		Debug.Log ("Loading Stage: " + stage);
		currentStage = stage;
		Application.LoadLevelAdditive (stage);
	}
}

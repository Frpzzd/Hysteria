using UnityEngine;
using System;
using System.Collections;
using DanmakuEngine.Core;

//Use this class to pass parameters from the menu to the gameplay scene
public class Global
{
	static Global()
	{
		GameStateChange (GameState.GameInitialize);
	}
	public static Rank Rank;

	private const int StartCredits = 3;
	public static int Credits;

	private static GameState gameState;
	public static GameState GameState
	{
		get { return gameState; }
	}

	public static GameType GameType = GameType.Normal;

	public static Pickup.State defaultPickupState = Pickup.State.Normal;
	
	private static float cachedTimeScale = 1f;

	public static void GameStateChange(GameState newState)
	{
		gameState = newState;
		switch(gameState)
		{
			case GameState.GameInitialize:
				Credits = StartCredits;
				defaultPickupState = Pickup.State.Normal;
				GameStateChange(GameState.InGame);
				return;
			case GameState.Paused:
				cachedTimeScale = Time.timeScale;
				Time.timeScale = 0f;
				break;
			case GameState.InGame:
				Time.timeScale = cachedTimeScale;
				break;
			case GameState.ZeroLives:
				cachedTimeScale = Time.timeScale;
				Time.timeScale = 0f;
				if(ScoreManager.CheckHighScore())
				{
					GameStateChange(GameState.HighScoreEntry);
				}
				else
				{
					GameStateChange(GameState.GameOver);
				}
				break;
			case GameState.GameOver:
				InGameMenuHandler.ZeroLives();
				break;
			case GameState.Continue:
				Credits--;
				Time.timeScale = cachedTimeScale;
				Player.Instance.lives = 3;
				ScoreManager.Continue();
				GameStateChange(GameState.InGame);
				break;
		}
	}

	public static IEnumerator WaitForUnpause()
	{
		while(GameState == GameState.Paused)
		{
			yield return new WaitForFixedUpdate();
		}
	}
}

public enum GameState { GameInitialize, InGame, Paused, ZeroLives, HighScoreEntry, GameOver, Continue}
public enum GameType {Normal, StagePractice, AttackPractice}
public enum Rank : int { Easy = 0, Normal = 1, Hard = 2, Insane = 4 }
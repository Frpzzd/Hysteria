using UnityEngine;
using System;
using System.Collections;

//Use this class to pass parameters from the menu to the gameplay scene
public class Global
{
	public static Rank Rank;

	private static uint credits;
	public static uint Credits
	{
		get { return credits; }
	}

	private static GameState gameState = GameState.MainMenu;
	public static GameState GameState
	{
		get { return gameState; }
	}

	public static GameType GameType = GameType.Normal;

	public static Pickup.PickupState defaultPickupState = Pickup.PickupState.Normal;

	public static void GameStateChange(GameState newState)
	{
		gameState = newState;
		switch(gameState)
		{
			case GameState.GameInitialize:
				credits = 3;
				defaultPickupState = Pickup.PickupState.Normal;
				GameStateChange(GameState.InGame);
				return;
			case GameState.CreditEnd:
				if(ScoreManager.CheckHighScore())
				{
					GameStateChange(GameState.HighScoreEntry);
					return;
				}
				return;
		}
	}
}

public enum GameState { MainMenu, GameInitialize, InGame, Paused, CreditEnd, HighScoreEntry, GameOver }
public enum GameType {Normal, StagePractice, AttackPractice}
public enum Rank : int { Easy = 0, Normal = 1, Hard = 2, Insane = 3 }
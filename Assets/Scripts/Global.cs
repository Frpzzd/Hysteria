using UnityEngine;
using System;
using System.Collections;

//Use this class to pass parameters from the menu to the gameplay scene
public class Global : StaticGameObject<Global>
{
	public static Rank Rank;

	private static uint credits;
	public static uint Credits
	{
		get { return credits; }
	}

	private static GameState gameState = GameState.Initialize;
	public static GameState GameState
	{
		get { return gameState; }
	}

	public static Pickup.PickupState defaultPickupState = Pickup.PickupState.Normal;

	public static void GameStateChange(GameState newState)
	{
		gameState = newState;
		switch(gameState)
		{
			case GameState.Initialize:
				credits = 3;
				defaultPickupState = Pickup.PickupState.Normal;
				return;
			case GameState.CreditEnd:
				if(ScoreManager.CheckHighScore())
				{
					GameStateChange(GameState.HighScoreEntry);
					return;
				}
				return;
		}
		GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject> ();
		Debug.Log (gameState.ToString ());
		foreach(GameObject go in allObjects)
		{
			go.SendMessage(gameState.ToString());
		}
	}
}

public enum GameState { Initialize, InGame, Paused, CreditEnd, HighScoreEntry, GameOver }
public enum GameType {Normal, StagePractice, AttackPractice}
public enum Rank : int { Easy = 0, Normal = 1, Hard = 2, Insane = 3 }
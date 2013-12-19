using UnityEngine;
using System;
using System.Collections;

public class Global : MonoBehaviour
{
	public static Rank Rank;

	public static ulong Score;
	public static uint PointValue;
	public static uint Graze;
	public static uint Credits;

	public static void Reset()
	{
		Score = Graze = 0;
		PointValue = 10000;
	}

	public static GameState gameState = GameState.MainMenu;
	public static Pickup.PickupState defaultPickupState = Pickup.PickupState.Normal;
}

public enum GameState { MainMenu, CharacterSelect, PauseMenu, InGame, GameOver }
public enum GameType {Normal, StagePractice, AttackPractice}
public enum Rank : int { Easy = 0, Normal = 1, Hard = 2, Insane = 3 }

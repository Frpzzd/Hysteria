using UnityEngine;
using System;
using System.Collections;

public static class Global
{
	public static int Rank;

	public static ulong Score;
	public static uint PointValue;
	public static uint Graze;

	public static void Reset()
	{
		Score = Graze = 0;
		PointValue = 10000;
	}

	public static GameState gameState;
	public static Pickup.PickupState defaultPickupState = Pickup.PickupState.Normal;
}

public enum GameState {Start, In_Game, Game_Over}

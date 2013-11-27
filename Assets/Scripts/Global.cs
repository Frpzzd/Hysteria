using UnityEngine;
using System;
using System.Collections;

public static class Global
{
	public static float TimePerFrame;

	public static Transform MainCam;

	public static float SpellOverlayDelay;
	public static int Rank;

	public static ulong Score;
	public static uint PointValue;
	public static uint Graze;

	public static void Reset()
	{
		Score = Graze = 0;
		PointValue = 5000;
	}

	public static GameState gameState;
}

public enum GameState {Start, In_Game, Game_Over}

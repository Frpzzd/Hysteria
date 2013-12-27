using UnityEngine;
using System;
using System.Collections;

public class ScoreManager : StaticGameObject<ScoreManager>
{
	public int[] defaultHighScores;
	private HighScore[] scores;

	public int defaultPointValue;
	public int pointValuePerPickup;
	public int pointValuePerGraze;
	public float powerToPointRatio;
	public int extraLifeBonus;
	public int extraBombBonus;

	private static ulong score;
	public static ulong Score
	{
		get { return score; }
	}

	private static uint pointValue;
	public static uint PointValue
	{
		get { return pointValue; }
	}

	public static uint graze;
	public static uint Graze
	{
		get { return graze; }
	}

	public static void Reset()
	{
		score = 0ul;
		graze = 0u;
		pointValue = (uint)instance.defaultPointValue;
	}

	public override void Awake()
	{
		base.Awake ();
		scores = new HighScore[defaultHighScores.Length];
		for(int i = 0; i < scores.Length; i++)
		{
			scores[i] = new HighScore(i + 1, defaultHighScores[i]);
		}
		pointValue = (uint)defaultPointValue;
	}

	void OnGUI()
	{
		if(Global.GameState == GameState.HighScoreEntry)
		{

		}
	}

	public static bool CheckHighScore()
	{
		return score > instance.scores [instance.scores.Length - 1].Score;
	}

	public static void ExtraLife()
	{
		score += (ulong)instance.extraLifeBonus;
	}

	public static void ExtraBomb()
	{
		score += (ulong)instance.extraBombBonus;
	}

	public static void PowerPickup()
	{
		score += (ulong)(instance.powerToPointRatio * (float)PointValue);
	}

	public static void PointPickup()
	{
		score += (ulong)PointValue;
	}

	public static void PointValuePickup()
	{
		pointValue += (uint)instance.pointValuePerPickup;
	}

	public static void GrazeBullet()
	{
		graze++;
		pointValue += (uint)instance.pointValuePerGraze;
	}

	private class HighScore
	{
		public bool empty = true;
		private int place;
		private ulong defaultScore;
		private string name;
		public string Name
		{
			get
			{
				return (name == null) ? "----------" : name;
			}

			set
			{
				name = value;
			}
		}

		private string score;
		public ulong Score
		{
			get
			{
				return (score == null) ? defaultScore : ulong.Parse(score);
			}

			set
			{
				score = value.ToString();
			}
		}

		private string finalStage;
		public string FinalStage
		{
			get
			{
				return (finalStage == null) ? "-----" : finalStage;
			}
		}

		private string timestamp;
		public string TimeStamp
		{
			get
			{
				return (timestamp == null) ? "--/--/-- --:--" : timestamp;
			}

			set
			{
				timestamp = value;
			}
		}

		private string Prefix
		{
			get { return "High Score" + place + " "; }
		}

#pragma warning disable 168
		public HighScore(int place, int defaultScore)
		{
			this.place = place;
			this.defaultScore = (ulong)defaultScore;
			try
			{
				name = TryString("name");
				score = TryString("score");
				finalStage = TryString("finalStage");
				timestamp = TryString("timestamp");
			}
			catch(HighScoreException ppe)
			{
				name = null;
				score = null;
				finalStage = null;
				timestamp = null;
			}
		}
#pragma warning restore 168

		public void Save()
		{
			if(name != null && score != null && finalStage != null && timestamp != null)
			{
				PlayerPrefs.SetString(Prefix + "name", name);
				PlayerPrefs.SetString(Prefix + "score", score);
				PlayerPrefs.SetString(Prefix + "finalStage", finalStage);
				PlayerPrefs.SetString(Prefix + "timestamp", timestamp);
			}
			else
			{
				Debug.Log("High Score Save Unsuccesful");
			}
		}

		private string TryString(string propName)
		{
			if(PlayerPrefs.HasKey(Prefix + propName))
			{
				return PlayerPrefs.GetString(Prefix + propName);
			}
			else
			{
				throw new HighScoreException();
			}
		}
		
		private class HighScoreException : SystemException {}
	}
}

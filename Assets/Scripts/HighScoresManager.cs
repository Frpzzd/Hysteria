using UnityEngine;
using System;
using System.Collections;

public class HighScoresManager : MonoBehaviour 
{
	private static HighScoresManager instance;
	public int[] defaultHighScores;
	private bool showGUI = false;
	private HighScore[] scores;

	void Awake()
	{
		if(instance != null)
		{
			Destroy(gameObject);
			return;
		}
		instance = this;
		scores = new HighScore[defaultHighScores.Length];
		for(int i = 0; i < scores.Length; i++)
		{
			scores[i] = new HighScore(i + 1, defaultHighScores[i]);
		}
	}

	void OnGUI()
	{
		if(showGUI)
		{

		}
	}

	public static void ShowGUI()
	{
		instance.showGUI = true;
	}

	public static void HideGUI ()
	{
		instance.showGUI = false;
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

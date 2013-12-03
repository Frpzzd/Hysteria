using UnityEngine;
using System.Collections;

public class ScoreGUI : MonoBehaviour 
{
	private GUIText gt;
	private ulong currentScore;
	private bool started;
	private ulong intermediate;
	private float lerp;

	void Awake()
	{
		gt = guiText;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Global.Score != currentScore)
		{
			if(!started)
			{
				lerp = 0f;
				started = true;
			}
			else
			{
				lerp += Time.deltaTime;
				if(lerp >= 1f)
				{
					started = false;
					currentScore = Global.Score;
					gt.text = Global.Score.ToString("0,000,000,000");
				}
				else
				{
					intermediate = (ulong)Mathf.Lerp(currentScore, Global.Score, lerp);
					gt.text = intermediate.ToString("0,000,000,000");
				}
			}
		}
	}
}

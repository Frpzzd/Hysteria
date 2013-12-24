using UnityEngine;
using System;
using System.Collections;

public class PauseMenu : Menu
{
	[NonSerialized]
	public AudioClip pauseClip;
	private float cachedTimeScale;

	public override void Toggle(bool value)
	{
		base.Toggle (value);
		if(value)
		{
			cachedTimeScale = Time.timeScale;
			Time.timeScale = 0f;
			SoundManager.PlaySoundEffect (pauseClip, 1f, Vector3.zero, false);
			SoundManager.PauseMusic ();
			Global.GameStateChange(GameState.Paused);
		}
		else
		{
			Time.timeScale = cachedTimeScale;
			SoundManager.UnpauseMusic ();
			Global.GameStateChange(GameState.InGame);
		}
	}
}

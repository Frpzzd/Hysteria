using UnityEngine;
using System;
using System.Collections;

public class PauseMenu : MonoBehaviour 
{
	public GameObject[] disableOnMenu;
	public GameObject[] enableOnMenu;
	[NonSerialized]
	public AudioClip pauseClip;
	private float cachedTimeScale;

	private enum GUIType { None, Pause, CreditEnd, GameOver }
	private GUIType guiType = GUIType.None;

	private void Toggle(bool value)
	{
		for(int i = 0; i < disableOnMenu.Length; i++)
		{
			disableOnMenu[i].SetActive(!value);
		}
		for(int i = 0; i < enableOnMenu.Length; i++)
		{
			enableOnMenu[i].SetActive(value);
		}
	}

	private void Pause()
	{
		Toggle(true);
		cachedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		SoundManager.PlaySoundEffect (pauseClip, 1f, Vector3.zero, false);
		SoundManager.PauseMusic ();
	}

	private void Unpause()
	{
		Toggle(false);
		Time.timeScale = cachedTimeScale;
		SoundManager.UnpauseMusic ();
	}

	void Update()
	{
		switch(Global.GameState)
		{
			case GameState.InGame:
				if(Input.GetButtonDown("Pause"))
				{
					Global.GameStateChange(GameState.Paused);
					Pause ();
				}
				break;
			case GameState.Paused:
				if(Input.GetButtonDown("Pause"))
				{
					Global.GameStateChange(GameState.InGame);
					Unpause();
				}
				break;
		}
	}

	void OnGUI()
	{
		switch(guiType)
		{
			default:
				return;
		}
	}
}

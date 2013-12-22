using UnityEngine;
using System;
using System.Collections;

public class PauseMenu : MonoBehaviour 
{
	public GameObject[] disableOnMenu;
	public GameObject[] enableOnMenu;
	[NonSerialized]
	public bool menuEnabled = false;
	public AudioClip pauseClip;
	private float cachedTimeScale;

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
		menuEnabled = true;
		Toggle(true);
		cachedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		SoundManager.PauseMusic ();
	}

	private void Unpause()
	{
		menuEnabled = false;
		Toggle(false);
		Time.timeScale = cachedTimeScale;
		SoundManager.UnpauseMusic ();
	}

	void Update()
	{
		if(Input.GetButtonDown("Pause") && !menuEnabled)
		{
			Pause ();
		}
		else if(Input.GetButtonDown("Pause") && menuEnabled)
		{
			Unpause();
		}
	}

	void OnGUI()
	{
		if (menuEnabled)
		{

		}
	}
}

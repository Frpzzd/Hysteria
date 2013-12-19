using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour 
{
	private GameState cachedGameState;
	public GameObject[] disableOnMenu;
	public GameObject[] enableOnMenu;
	private float cachedTimeScale;
	private bool menuEnabled;

	// Use this for initialization
	void Start () 
	{
		cachedGameState = Global.gameState;
		menuEnabled = false;
		cachedTimeScale = 1f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(cachedGameState != Global.gameState)
		{
			menuEnabled = Global.gameState == GameState.PauseMenu;
			for(int i = 0; i < disableOnMenu.Length; i++)
			{
				disableOnMenu[i].SetActive(!menuEnabled);
			}
			for(int i = 0; i < enableOnMenu.Length; i++)
			{
				enableOnMenu[i].SetActive(menuEnabled);
			}
			if(menuEnabled)
			{
				cachedTimeScale = Time.timeScale;
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = cachedTimeScale;
			}
		}
		cachedGameState = Global.gameState;
	}

	void OnGUI()
	{
		if(menuEnabled)
		{
			//Pause Menu GUI here
		}
	}
}

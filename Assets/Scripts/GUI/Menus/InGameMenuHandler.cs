using UnityEngine;
using System.Collections.Generic;

public class InGameMenuHandler : StaticGameObject<InGameMenuHandler>
{
	public InGameMenu pauseMenu;
	public InGameMenu creditEndMenu;
	public InGameMenu gameOverMenu;
	private InGameMenu[] menus;
	[HideInInspector]
	public InGameMenu currentMenu;
	public AudioClip gameOverTheme = null;
	public AudioClip menuMoveClip = null;
	public AudioClip menuInvalidClip = null;
	public AudioClip menuSelectClip = null;
	public AudioClip pauseClip = null;
	private bool downButtonDown = false;
	private bool upButtonDown = false;
	private bool leftButtonDown = false;
	private bool rightButtonDown = false;
	public GUITexture background;
	
	private const float baseScreenWidth = 1024f;
	private const float baseScreenHeight = 768f;
	
	public override void Awake ()
	{
		base.Awake ();
		menus = new InGameMenu[] { pauseMenu, gameOverMenu, creditEndMenu };
	}
	
	public static void ChangeBackground(Texture2D texture)
	{
		if(texture !=  null)
		{
			Instance.background.texture = texture;
		}
	}

	public static void ZeroLives()
	{
		for(int i = 0; i < Instance.menus.Length; i++)
		{
			Instance.menus[i].Toggle(Instance.menus[i] == (Global.Credits > 0) ? Instance.creditEndMenu : Instance.gameOverMenu);
		}
	}
	
	void Update()
	{
		if(Input.GetButtonDown("Pause"))
		{
			Debug.Log(Global.GameState);
			if(Global.GameState == GameState.InGame)
			{
				Global.GameStateChange(GameState.Paused);
				SoundManager.PlaySoundEffect (pauseClip, 1f, Vector3.zero, false);
				SoundManager.PauseMusic ();
			}
			else if(Global.GameState == GameState.Paused)
			{
				Global.GameStateChange(GameState.InGame);
				SoundManager.UnpauseMusic ();
			}
		}
		
		switch(Global.GameState)
		{
		case GameState.Paused:
			currentMenu = pauseMenu;
			break;
		case GameState.InGame:
			currentMenu = null;
			break;
		case GameState.GameOver:
			if(Global.Credits > 0)
			{
				currentMenu = creditEndMenu;
			}
			else
			{
				currentMenu = gameOverMenu;
			}
			break;
		}

		background.enabled = currentMenu != null;

		if (currentMenu != null) 
		{
			for(int i = 0; i < menus.Length; i++)
			{
				menus[i].Toggle(menus[i] == currentMenu);
			}
			
			if(CheckDown())
			{
				if(currentMenu.OnDown())
				{
					SoundManager.PlaySoundEffect(Instance.menuMoveClip);
				}
			}
			if(CheckUp())
			{
				if(currentMenu.OnUp())
				{
					SoundManager.PlaySoundEffect(Instance.menuMoveClip);
				}
			}
			if(CheckLeft())
			{
				if(currentMenu.OnLeft())
				{
					SoundManager.PlaySoundEffect(Instance.menuMoveClip);
				}
			}
			if(CheckRight())
			{
				if(currentMenu.OnRight())
				{
					SoundManager.PlaySoundEffect(Instance.menuMoveClip);
				}
			}
			if(Input.GetButtonDown("Pause") || Input.GetButtonDown("Bomb"))
			{
				if(currentMenu.ReturnToPrevious())
				{
					SoundManager.PlaySoundEffect(menuSelectClip);
				}
			}
			if(Input.GetButtonDown("Shoot"))
			{
				if(currentMenu.Select())
				{
					SoundManager.PlaySoundEffect(menuSelectClip);
				}
			}	
		}
		else
		{
			for(int i = 0; i < menus.Length; i++)
			{
				menus[i].Toggle(false);
			}
		}
	}
	
	private bool CheckDown()
	{
		if(Input.GetAxisRaw("Vertical") < 0)
		{
			if(!downButtonDown)
			{
				downButtonDown = true;
				return true;
			}
		}
		else
		{
			downButtonDown = false;
		}
		return false;
	}
	
	private bool CheckUp()
	{
		if(Input.GetAxisRaw("Vertical") > 0)
		{
			if(!upButtonDown)
			{
				upButtonDown = true;
				return true;
			}
		}
		else
		{
			upButtonDown = false;
		}
		return false;
	}
	
	private bool CheckLeft()
	{
		if(Input.GetAxisRaw("Horizontal") < 0)
		{
			if(!leftButtonDown)
			{
				leftButtonDown = true;
				return true;
			}
		}
		else
		{
			leftButtonDown = false;
		}
		return false;
	}
	
	private bool CheckRight()
	{
		if(Input.GetAxisRaw("Horizontal") > 0)
		{
			if(!rightButtonDown)
			{
				rightButtonDown = true;
				return true;
			}
		}
		else
		{
			rightButtonDown = false;
		}
		return false;
	}
	
	public static void ScaleTextSize(GUIStyle style, int originalFontSize)
	{
		Vector2 scale = new Vector2 (Screen.width / baseScreenWidth, Screen.height / baseScreenHeight);
		style.fontSize = Mathf.FloorToInt (scale.y * originalFontSize);
	}
}

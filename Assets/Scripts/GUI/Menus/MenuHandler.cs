using UnityEngine;
using System.Collections.Generic;

public class MenuHandler : StaticGameObject<MenuHandler>
{
	public Menu currentMenu;
	public AudioClip menuMoveClip = null;
	public AudioClip menuInvalidClip = null;
	public AudioClip menuSelectClip = null;
	private static PauseMenu pauseMenu;
	private bool downButtonDown = false;
	private bool upButtonDown = false;
	private bool leftButtonDown = false;
	private bool rightButtonDown = false;
	public GameObject background;

	private const float baseScreenWidth = 1024f;
	private const float baseScreenHeight = 768f;

	public override void Awake ()
	{
		base.Awake ();
		pauseMenu = GameObject.FindObjectOfType<PauseMenu> ();
		ChangeMenu (currentMenu);
	}

	public static void ChangeMenu(Menu menu)
	{
		instance.currentMenu.Toggle (false);
		instance.currentMenu = menu;
		instance.currentMenu.Toggle (true);
	}

	public static void ChangeBackground(Texture2D texture)
	{
		if(texture !=  null)
		{
			instance.background.renderer.material.mainTexture = texture;
			instance.background.SetActive(true);
		}
		else
		{
			instance.background.SetActive(false);
		}
	}

	void Update()
	{
		if(CheckDown())
		{
			if(currentMenu.MoveDown())
			{
				SoundManager.PlaySoundEffect(instance.menuMoveClip);
			}
		}
		if(CheckUp())
		{
			if(currentMenu.MoveUp())
			{
				SoundManager.PlaySoundEffect(instance.menuMoveClip);
			}
		}
		if(CheckLeft())
		{
			if(currentMenu.SlideOptionLeft())
			{
				SoundManager.PlaySoundEffect(instance.menuMoveClip);
			}
		}
		if(CheckRight())
		{
			if(currentMenu.SlideOptionRight())
			{
				SoundManager.PlaySoundEffect(instance.menuMoveClip);
			}
		}
		if(Global.GameState != GameState.InGame && Global.GameState != GameState.Paused)
		{
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
			if(Input.GetButtonDown("Pause"))
			{
				if(Global.GameState == GameState.InGame)
				{
					pauseMenu.Toggle(true);
				}
				else
				{
					pauseMenu.Toggle(false);
				}
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

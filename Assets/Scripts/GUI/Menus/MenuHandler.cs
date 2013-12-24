using UnityEngine;
using System.Collections.Generic;

public class MenuHandler : StaticGameObject<MenuHandler>
{
	public Menu currentMenu;
	public AudioClip menuMoveClip = null;
	public AudioClip menuInvalidClip = null;
	public AudioClip menuSelectClip = null;
	private static List<Menu> menus;
	private static PauseMenu pauseMenu;
	private bool downButtonDown = false;
	private bool upButtonDown = false;

	private const float baseScreenWidth = 1024f;
	private const float baseScreenHeight = 768f;

	public override void Awake ()
	{
		base.Awake ();
		Screen.lockCursor = true;
		menus = new List<Menu>((Menu[])GameObject.FindObjectsOfType<Menu> ());
		pauseMenu = GameObject.FindObjectOfType<PauseMenu> ();
		menus.Remove (pauseMenu);
		ChangeMenu (currentMenu);
	}

	public static void ChangeMenu(Menu menu)
	{
		Debug.Log (menu.GetType());
		instance.currentMenu.Toggle (false);
		instance.currentMenu = menu;
		instance.currentMenu.Toggle (true);
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
		if(Global.GameState != GameState.InGame && Global.GameState != GameState.Paused)
		{
			if(Input.GetButtonDown("Pause") || Input.GetButtonDown("Bomb"))
			{
				SoundManager.PlaySoundEffect(menuSelectClip);
				currentMenu.ReturnToParent();
			}
			if(Input.GetButtonDown("Shoot"))
			{
				SoundManager.PlaySoundEffect(menuSelectClip);
				currentMenu.Select();
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

	public static void ScaleTextSize(GUIStyle style, int originalFontSize)
	{
		Vector2 scale = new Vector2 (Screen.width / baseScreenWidth, Screen.height / baseScreenHeight);
		style.fontSize = Mathf.FloorToInt (scale.y * originalFontSize);
	}
}

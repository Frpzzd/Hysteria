using UnityEngine;
using System.Collections.Generic;

public class MenuHandler : StaticGameObject<MenuHandler>
{
	public Menu currentMenu;
	public AudioClip menuMoveClip = null;
	public AudioClip menuInvalidClip = null;
	public AudioClip menuSelectClip = null;
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
		ChangeMenu (currentMenu);
	}

	public static void ChangeMenu(Menu menu)
	{
		Instance.currentMenu.Toggle (false);
		Instance.currentMenu = menu;
		Instance.currentMenu.Toggle (true);
	}

	public static void ChangeBackground(Texture2D texture)
	{
		if(texture !=  null)
		{
			Instance.background.texture = texture;
		}
	}

	void Update()
	{
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

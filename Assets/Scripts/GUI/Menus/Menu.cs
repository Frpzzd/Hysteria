using UnityEngine;
using System.Collections;
using JamesLib;

[System.Serializable]
public abstract class Menu : CachedObject
{
	public enum Orientation { Vertical, Horizontal }
	public Orientation orientation;
	public Texture2D backgroundImage;
	public MenuOption[] children;
	[System.NonSerialized]
	public Menu previousMenu;
	[HideInInspector]
	public MenuHandler handler;
	protected int selectedIndex = 0;

	protected virtual void OnChildSwitchImpl(int i) { }
	protected virtual void OnReturnToPreviousImpl() { }

	public void OnChildSwitch(int i)
	{
		OnChildSwitchImpl (i);
		if(children[i].targetMenu != null)
		{
			children[i].targetMenu.previousMenu = this;
			MenuHandler.ChangeMenu(children[i].targetMenu);
		}
	}

	public virtual void Update()
	{
		for(int i = 0; i < children.Length; i++)
		{
			children[i].isSelected = i == selectedIndex;
		}
	}

	public bool OnReturnToPrevious()
	{
		OnReturnToPreviousImpl ();
		if(previousMenu != null)
		{
			MenuHandler.ChangeMenu(previousMenu);
			return true;
		}
		return false;
	}

	public virtual void Toggle(bool value)
	{
		if(value)
		{
			MenuHandler.ChangeBackground(backgroundImage);
		}
		if (value)
		{
			OnMenuEnter();
		}
		else
		{
			OnMenuExit();
		}
		GameObject.SetActive (value);
	}

	public virtual void OnMenuEnter()
	{
	}

	public virtual void OnMenuExit()
	{
	}

	public bool OnUp()
	{
		if(orientation == Orientation.Vertical)
		{
			return PreviousOption();
		}
		else
		{
			return SlideOptionDecrease();
		}
	}

	public bool OnDown()
	{
		if(orientation == Orientation.Vertical)
		{
			return NextOption();
		}
		else
		{
			return SlideOptionIncrease();
		}
	}

	public bool OnLeft()
	{
		if(orientation == Orientation.Horizontal)
		{
			return PreviousOption();
		}
		else
		{
			return SlideOptionDecrease();
		}
	}

	public bool OnRight()
	{
		if(orientation == Orientation.Horizontal)
		{
			return NextOption();
		}
		else
		{
			return SlideOptionIncrease();
		}
	}

	public virtual bool ReturnToPrevious()
	{
		return OnReturnToPrevious ();
	}

	public bool Select()
	{
		OnChildSwitch (selectedIndex);
		return !children [selectedIndex].HasOptions;
	}

	private bool PreviousOption()
	{
		bool success = (selectedIndex - 1 >= 0);
		selectedIndex = (success) ? selectedIndex - 1 : 0;
		return success;
	}

	private bool NextOption()
	{
		bool success = (selectedIndex + 1 < children.Length);
		selectedIndex = (success) ? selectedIndex + 1 : children.Length - 1;
		return success;
	}

	private bool SlideOptionIncrease()
	{
		children[selectedIndex].ShiftLeft();
		return children[selectedIndex].HasOptions;
	}

	private bool SlideOptionDecrease()
	{
		children[selectedIndex].ShiftRight();
		return children[selectedIndex].HasOptions;
	}
}

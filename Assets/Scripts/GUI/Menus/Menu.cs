using UnityEngine;
using System;
using System.Collections;

[Serializable]
public abstract class Menu : MonoBehaviour
{
	public Texture2D backgroundImage;
	public GUIStyle menuStyle;
	public ChildMenu title;
	public ChildMenu[] children;
	public GameObject[] relatedObjects;
	public GameObject[] unrelatedObjects;
	[HideInInspector]
	public Menu previousMenu;
	protected int selectedIndex = 0;

	protected virtual void OnChildSwitchImpl(int i) { }
	protected virtual void OnReturnToPreviousImpl() { }

	private int originalStyleFontSize;

	public void OnChildSwitch(int i)
	{
		OnChildSwitchImpl (i);
		if(children[i].menu != null)
		{
			children[i].menu.previousMenu = this;
			MenuHandler.ChangeMenu(children[i].menu);
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
		for(int i = 0; i < unrelatedObjects.Length; i++)
		{
			unrelatedObjects[i].SetActive(!value);
		}
		for(int i = 0; i < relatedObjects.Length; i++)
		{
			relatedObjects[i].SetActive(value);
		}
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
		enabled = value;
	}

	public virtual void OnMenuEnter()
	{
	}

	public virtual void OnMenuExit()
	{
	}

	public virtual bool ReturnToPrevious()
	{
		return OnReturnToPrevious ();
	}

	public bool Select()
	{
		OnChildSwitch (selectedIndex);
		return !children [selectedIndex].selection;
	}

	public virtual void Awake()
	{
		originalStyleFontSize = menuStyle.fontSize;
	}

	public bool MoveUp()
	{
		bool success = (selectedIndex - 1 >= 0);
		selectedIndex = (success) ? selectedIndex - 1 : 0;
		return success;
	}

	public bool MoveDown()
	{
		bool success = (selectedIndex + 1 < children.Length);
		selectedIndex = (success) ? selectedIndex + 1 : children.Length - 1;
		return success;
	}

	public virtual void OnGUI()
	{
		MenuHandler.ScaleTextSize (menuStyle, originalStyleFontSize);
		for(int i = 0; i < children.Length; i++)
		{
			children[i].Draw(menuStyle);
		}
		children[selectedIndex].Focus();
	}

	public bool SlideOptionLeft()
	{
		children[selectedIndex].ShiftLeft();
		return children[selectedIndex].selection;
	}

	public bool SlideOptionRight()
	{
		children[selectedIndex].ShiftRight();
		return children[selectedIndex].selection;
	}

	[Serializable]
	public class ChildMenu
	{
		public Menu menu;
		public string controlName;
		public Rect screenRect;
		public bool selection { get { return content.Length > 1; } }
		public bool useCustomStyle;
		public GUIStyle customStyle;
		public GUIContent[] content;
		public int selected = 0;

		private int originalCustomFontSize;

		public void Initialize()
		{
			originalCustomFontSize = customStyle.fontSize;
		}

		public GUIContent Content
		{
			get { return (content.Length >= 1) ? content [selected] : new GUIContent(); }
		}

		public void ShiftLeft()
		{
			if(selection)
			{
				selected--;
				if(selected < 0)
				{
					selected = content.Length - 1;
				}
			}
		}

		public void ShiftRight()
		{
			if(selection)
			{
				selected++;
				if(selected >= content.Length)
				{
					selected = 0;
				}
			}
		}

		public void  Draw(GUIStyle menuStyle)
		{
			if(content.Length >= 1)
			{
				if(useCustomStyle)
				{
					MenuHandler.ScaleTextSize(customStyle, originalCustomFontSize);
				}
				GUI.SetNextControlName(controlName);
				GUI.Button (ScreenRect(screenRect), Content, (useCustomStyle) ? customStyle : menuStyle);
			}
		}

		public void Focus()
		{
			GUI.FocusControl (controlName);
		}

		private static Rect ScreenRect(Rect input)
		{
			return new Rect (input.x * Screen.width, input.y * Screen.height, input.width * Screen.width, input.height * Screen.height);
		}
	}
}

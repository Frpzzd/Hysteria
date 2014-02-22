using UnityEngine;
using System.Collections;
using JamesLib;

[System.Serializable]
public abstract class InGameMenu : CachedObject
{
	public enum Orientation { Vertical, Horizontal }
	public Orientation orientation;
	public MenuOption[] children;
	[System.NonSerialized]
	public Menu previousMenu;
	[HideInInspector]
	public MenuHandler handler;
	protected int selectedIndex = 0;
	
	protected virtual void OnChildSwitchImpl(int i) { }
	protected virtual void OnReturnToPreviousImpl() { }
	
	public virtual void OnChildSwitch(int i)
	{
	}
	
	public virtual void Update()
	{
		for(int i = 0; i < children.Length; i++)
		{
			children[i].isSelected = i == selectedIndex;
		}
	}
	
	public virtual bool OnReturnToPrevious()
	{
		return false;
	}
	
	public virtual void Toggle(bool value)
	{
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
	
	[System.Serializable]
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

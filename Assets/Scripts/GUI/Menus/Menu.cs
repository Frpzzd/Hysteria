using UnityEngine;
using System;
using System.Collections;

[Serializable]
public abstract class Menu : MonoBehaviour
{
	public Texture2D backgroundImage;
	public GUIStyle menuStyle;
	public ChildMenu parent;
	public ChildMenu[] children;
	public GameObject[] relatedObjects;
	public GameObject[] unrelatedObjects;

	protected string[] buttonNames;
	protected int selectedIndex = 0;

	protected virtual void OnChildSwitchImpl(int i) { }
	protected virtual void OnParentSwitchImpl() { }

	private int originalStyleFontSize;

	public void OnChildSwitch(int i)
	{
		OnChildSwitchImpl (i);
		if(children[i].menu != null)
		{
			MenuHandler.ChangeMenu(children[i].menu);
		}
	}

	public void OnParentSwitch()
	{
		OnParentSwitchImpl ();
		if(parent.menu != null)
		{
			MenuHandler.ChangeMenu(parent.menu);
		}
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

	public virtual void ReturnToParent()
	{
		OnParentSwitch ();
	}

	public void Select()
	{
		if(selectedIndex == buttonNames.Length - 1 && parent.content.Length >= 1)
		{
			OnParentSwitch();
		}
		else
		{
			OnChildSwitch (selectedIndex);
		}
	}

	public virtual void Awake()
	{
		originalStyleFontSize = menuStyle.fontSize;
		buttonNames = new string[children.Length + ((parent.content.Length >= 1) ? 1 : 0)];
		for(int i = 0; i < children.Length; i++)
		{
			buttonNames[i] = children[i].controlName;
		}
		if(parent.content.Length >= 1)
		{
			buttonNames [buttonNames.Length - 1] = parent.controlName;
		}
	}

	public bool MoveUp()
	{
		bool success = (selectedIndex - 1 >= 0);
		selectedIndex = (success) ? selectedIndex - 1 : 0;
		return success;
	}

	public bool MoveDown()
	{
		bool success = (selectedIndex + 1 <  buttonNames.Length);
		selectedIndex = (success) ? selectedIndex + 1 : buttonNames.Length - 1;
		return success;
	}

	public virtual void OnGUI()
	{
		if(menuStyle != null)
		{
			MenuHandler.ScaleTextSize (menuStyle, originalStyleFontSize);
		}
		for(int i = 0; i < children.Length; i++)
		{
			if(children[i].content.Length >= 1)
			{
				GUI.SetNextControlName(children[i].controlName);
				GUI.Button (screenRect(children[i].screenRect), children[i].Content, menuStyle);
			}
		}
		if(parent.content.Length >= 1)
		{
			GUI.SetNextControlName (parent.controlName);
			GUI.Button (screenRect (parent.screenRect), parent.Content, menuStyle);
		}
		GUI.FocusControl (buttonNames[selectedIndex]);
	}

	public bool SlideOptionLeft()
	{
		if(selectedIndex > children.Length)
		{
			parent.ShiftLeft();
			return parent.selection;
		}
		else
		{
			children[selectedIndex].ShiftLeft();
			return children[selectedIndex].selection;
		}
	}

	public bool SlideOptionRight()
	{
		if(selectedIndex > children.Length)
		{
			parent.ShiftRight();
			return parent.selection;
		}
		else
		{
			children[selectedIndex].ShiftRight();
			return children[selectedIndex].selection;
		}
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

	}

	private Rect screenRect(Rect input)
	{
		return new Rect (input.x * Screen.width, input.y * Screen.height, input.width * Screen.width, input.height * Screen.height);
	}
}

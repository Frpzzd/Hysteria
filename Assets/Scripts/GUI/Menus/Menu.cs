using UnityEngine;
using System;
using System.Collections;

[Serializable]
public abstract class Menu : MonoBehaviour
{
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

	public virtual void OnChildSwitch(int i)
	{
		OnChildSwitchImpl (i);
		if(children[i].menu != null)
		{
			MenuHandler.ChangeMenu(children[i].menu);
		}
	}

	public virtual void OnParentSwitch()
	{
		Debug.Log ("Hello");
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
		enabled = value;
	}

	public virtual void ReturnToParent()
	{
		OnParentSwitch ();
	}

	public void Select()
	{
		if(selectedIndex == buttonNames.Length - 1)
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
		buttonNames = new string[children.Length + 1];
		for(int i = 0; i < children.Length; i++)
		{
			buttonNames[i] = children[i].content.text;
		}
		buttonNames [buttonNames.Length - 1] = parent.content.text;
	}

	public bool MoveUp()
	{
		bool success = (selectedIndex - 1 >= 0);
		selectedIndex = (success) ? selectedIndex - 1 : 0;
		return success;
	}

	public bool MoveDown()
	{
		bool success = (selectedIndex + 1 < buttonNames.Length);
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
			GUI.SetNextControlName(children[i].content.text);
			GUI.Button (screenRect(children[i].screenRect), children[i].content, menuStyle);
		}
		GUI.SetNextControlName (parent.content.text);
		GUI.Button (screenRect (parent.screenRect), parent.content, menuStyle);
		GUI.FocusControl (buttonNames [selectedIndex]);
	}

	[Serializable]
	public class ChildMenu
	{
		public Menu menu;
		public Rect screenRect;
		public GUIContent content;
	}

	private Rect screenRect(Rect input)
	{
		return new Rect (input.x * Screen.width, input.y * Screen.height, input.width * Screen.width, input.height * Screen.height);
	}
}

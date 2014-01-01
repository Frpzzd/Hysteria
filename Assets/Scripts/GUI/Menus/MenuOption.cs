using UnityEngine;
using System.Collections;

[System.Serializable]
[RequireComponent(typeof(GUIText))]
[RequireComponent(typeof(ScaleGUIText))]
public class MenuOption : MonoBehaviour 
{
	[SerializeField]
	public GUITexture background;
	[SerializeField]
	public Texture2D selectedBackground;
	[SerializeField]
	public Color selectedTextColor;
	[SerializeField]
	public Color selectedBackgroundColor;
	[SerializeField]
	public string[] additionalOptions;
	[SerializeField]
	public Menu targetMenu;

	public bool HasOptions
	{
		get { return options.Length > 1; }
	}
	
	[System.NonSerialized]
	public GUIText text;
	[System.NonSerialized]
	private Texture2D normalBackground;
	[System.NonSerialized]
	private Color normalBackgroundColor;
	[System.NonSerialized]
	private Color normalColor;
	[System.NonSerialized]
	public Menu parent;
	[System.NonSerialized]
	public bool isSelected;
	[System.NonSerialized]
	private string[] options;
	[System.NonSerialized]
	public int selection = 0;

	void Awake()
	{
		text = GetComponentInChildren<GUIText> ();
		normalBackground = (Texture2D)background.texture;
		normalBackgroundColor = background.color;
		normalColor = text.color;
		if(additionalOptions.Length > 0)
		{
			options = new string[additionalOptions.Length + 1];
			options[0] = text.text;
			for(int i = 0; i < additionalOptions.Length; i++)
			{
				options[i + 1] = additionalOptions[i];
			}
		}
		else
		{
			options = new string[] { text.text };
		}
	}

	void Update()
	{
		if(isSelected)
		{
			background.texture = selectedBackground;
			background.color = selectedBackgroundColor;
			text.color = selectedTextColor;
		}
		else
		{
			background.texture = normalBackground;
			background.color = normalBackgroundColor;
			text.color = normalColor;
		}
		text.text = options [selection];
	}

	public void ShiftLeft()
	{
		if(HasOptions)
		{
			selection--;
			if(selection < 0)
			{
				selection = options.Length - 1;
			}
		}
	}
	
	public void ShiftRight()
	{
		if(HasOptions)
		{
			selection++;
			if(selection >= options.Length)
			{
				selection = 0;
			}
		}
	}
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIText))]
public class ScaleGUIText : MonoBehaviour 
{
	public const float originalScreenWidth = 1024f;

	[System.NonSerialized]
	private int originalFontSize;
	[System.NonSerialized]
	private GUIText cachedGUIText;

	void Awake()
	{
		cachedGUIText = guiText;
		originalFontSize = cachedGUIText.fontSize;
	}
	
	// Update is called once per frame
	void Update () 
	{
		cachedGUIText.fontSize = (int)((float)originalFontSize * ((float)Screen.width / originalScreenWidth));
	}
}

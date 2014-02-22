using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GUIText))]
public class GUITextShadow : MonoBehaviour 
{
	[HideInInspector]
	public GameObject shadow;
	[HideInInspector]
	public GUIText shadowText;
	[HideInInspector]
	public GUIText text;

	public Vector2 offset;
	public Color shadowColor;

	void Start()
	{
		shadow = new GameObject();
		shadow.transform.parent = transform;
		shadow.name = "Shadow";
		shadowText = shadow.AddComponent<GUIText> ();
		text = GetComponent<GUIText> ();
	}

	void Update () 
	{
		shadowText.alignment = text.alignment;
		shadowText.anchor = text.anchor;
		shadowText.color = shadowColor;
		shadowText.font = text.font;
		shadowText.fontSize = text.fontSize;
		shadowText.fontStyle = text.fontStyle;
		shadowText.lineSpacing = text.lineSpacing;
		//shadowText.material = text.material;
		shadowText.pixelOffset = text.pixelOffset;
		shadowText.richText = text.richText;
		shadowText.tabSize = text.tabSize;
		shadowText.text = text.text;
		if (transform.position.x == 0 && transform.position.y == 0) 
		{

		}
		else
		{
			shadow.transform.position = transform.position + new Vector3(offset.x, offset.y, -1f);
		}
	}
}

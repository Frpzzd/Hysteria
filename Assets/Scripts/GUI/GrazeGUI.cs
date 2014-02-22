using UnityEngine;
using System.Collections;

public class GrazeGUI : MonoBehaviour 
{
	private GUIText gt;

	// Use this for initialization
	void Start () 
	{
		gt = guiText;
	}
	
	// Update is called once per frame
	void Update () 
	{
		gt.text = ScoreManager.Graze.ToString ("n0");
	}
}

using UnityEngine;
using System.Collections;

public class PowerGUI : MonoBehaviour 
{
	private GUIText gt;

	void Start()
	{
		gt = guiText;
	}

	// Update is called once per frame
	void Update () 
	{
		gt.text = Player.Power.ToString("0.00") + " / " + Player.MaxPower.ToString("0.00");
	}
}

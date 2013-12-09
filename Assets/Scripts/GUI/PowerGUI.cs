using UnityEngine;
using System.Collections;

public class PowerGUI : MonoBehaviour 
{
	private GUIText gt;
	private static float currentPower;
	private string maxPower;

	void Start()
	{
		currentPower = 0;
		maxPower = " / 4.00";
		gt = guiText;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Player.instance.power != currentPower)
		{
			gt.text = "Power: " + Player.instance.power.ToString("n2") + maxPower;
			currentPower = Player.instance.power;
		}
	}
}

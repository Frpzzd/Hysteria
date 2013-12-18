using UnityEngine;
using System.Collections;

public class PowerGUI : MonoBehaviour 
{
	private GUIText gt;
	private static float currentPower;

	void Start()
	{
		currentPower = 0;
		gt = guiText;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Player.instance.power != currentPower)
		{
			gt.text = ((float)Player.instance.power / (float)Player.instance.options.Length).ToString("0.00%");
			currentPower = Player.instance.power;
		}
	}
}

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
		if(Player.Power != currentPower)
		{
			gt.text = ((float)Player.Power / (float)Player.MaxOptions).ToString("0.00%");
			currentPower = Player.Power;
		}
	}
}

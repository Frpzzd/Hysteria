using UnityEngine;
using System.Collections;

public class PlayerBombsGUI : MonoBehaviour 
{
	public GUITexture[] bombTextures;
	
	void Update()
	{
		if(Player.Instance != null)
		{
			for(int i = 0; i < bombTextures.Length; i++)
			{
				bombTextures[i].enabled = ((i + 1) <= Player.Power / Player.BombCost);
			}
		}
	}
}

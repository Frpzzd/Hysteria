using UnityEngine;
using System.Collections;

public class PlayerLivesGUI : MonoBehaviour {

	public GUITexture[] lifeTextures;

	void Update()
	{
		if(Player.Instance != null)
		{
			for(int i = 0; i < lifeTextures.Length; i++)
			{
				lifeTextures[i].enabled = ((i + 1) <= Player.Instance.lives);
			}
		}
	}
}

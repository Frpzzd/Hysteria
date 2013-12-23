using UnityEngine;
using System.Collections;

public class MainMeniu : MonoBehaviour 
{
	void OnGUI()
	{
		if(GUILayout.Button("Start Game"))
		{
			Application.LoadLevel("Main Gameplay");
		}
	}
}

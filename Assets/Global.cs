using UnityEngine;
using System;
using System.Collections;

public static class Global
{
	public static class Control
	{
		public static KeyCode Up = KeyCode.UpArrow;
		public static KeyCode Down = KeyCode.DownArrow;
		public static KeyCode Left = KeyCode.LeftArrow;
		public static KeyCode Right = KeyCode.RightArrow;
		public static KeyCode Shoot = KeyCode.Z;
		public static KeyCode Focus = KeyCode.LeftShift;
		public static KeyCode Bomb = KeyCode.X; 

		static Control()
		{
			if(PlayerPrefs.HasKey("Key_Up"))
			{
				Up = (KeyCode)(KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Up"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Up", Up.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Down"))
			{
				Down = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Down"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Down", Down.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Left"))
			{
				Left = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Left"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Left", Left.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Right"))
			{
				Right = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Right"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Right", Right.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Shoot"))
			{
				Shoot = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Shoot"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Shoot", Shoot.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Focus"))
			{
				Focus = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Focus"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Focus", Focus.ToString());
			}
			if(PlayerPrefs.HasKey("Key_Bomb"))
			{
				Bomb = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Key_Bomb"));
			}
			else
			{
				PlayerPrefs.SetString("Key_Bomb", Bomb.ToString());
			}
		}
	}
}

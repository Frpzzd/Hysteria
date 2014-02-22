using UnityEngine;
using System.Collections;

namespace JamesLib
{
	[AddComponentMenu("Microfunctionality/Keep Object")]
	public class KeepObject : MonoBehaviour 
	{
		void Awake()
		{
			DontDestroyOnLoad (gameObject);
		}
	}
}


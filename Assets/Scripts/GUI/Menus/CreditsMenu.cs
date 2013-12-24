using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class CreditsMenu : Menu
{
	public CreditObject[] credits;

	[Serializable]
	public class CreditObject
	{
		public string name;
		public string[] work;
	}


}

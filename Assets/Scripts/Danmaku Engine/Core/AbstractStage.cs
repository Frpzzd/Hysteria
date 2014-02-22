using UnityEngine;
using System.Collections;
using JamesLib;

namespace DanmakuEngine.Core
{
	[System.Serializable]
	public abstract class AbstractStage : CachedObject
	{
		[SerializeField]
		public AudioClip theme;
		[SerializeField]
		public int bonus;

	}
}


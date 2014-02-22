using UnityEngine;
using System.Collections.Generic;
using DanmakuEngine.Core;

namespace DanmakuEngine.Core
{
	[AddComponentMenu("Danmaku Engine/Player Shot Pool")]
	public class PlayerShotPool : GameObjectPool<PlayerShot, PlayerShotPool>
	{
	}
}
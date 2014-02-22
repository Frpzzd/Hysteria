using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Actions;

namespace DanmakuEngine.Core
{
	[AddComponentMenu("Danmaku Engine/Bullet Pool")]
	public class BulletPool : GameObjectPool<Bullet, BulletPool>
	{
	}
}
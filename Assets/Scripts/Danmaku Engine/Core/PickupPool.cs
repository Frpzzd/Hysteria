using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DanmakuEngine.Core
{
	[AddComponentMenu("Danmaku Engine/Pickup Pool")]
	public class PickupPool : GameObjectPool<Pickup, PickupPool>
	{
		public static PickupPool PoolInstance
		{
			get { return Instance as PickupPool; }
		}
		
		public Sprite pointSprite;
		public Sprite powerSprite;
		public Sprite lifeSprite;
		public Sprite pointValueSprite;
		
		public Color pointColor;
		public Color powerColor;
		public Color lifeColor;
		public Color pointValueColor;
		public Color fullPowerColor;

		public float bigPowerScale;
		
		public float InitialVelocity;
		public float MaximumDownwardVelocity;
		public float Acceleration;
		public float RotationSpeed;
		
		public float AutoCollectSpeed;
		public float ProximityCollectSpeed;
		
		public Pickup Get (Pickup.Type param)
		{
			Pickup p = base.Get ();
			switch(param)
			{
			case Pickup.Type.Point:
				p.render.sprite = pointSprite;
				p.render.color = pointColor;
				break;
			case Pickup.Type.Power:
				p.render.sprite = powerSprite;
				p.render.color = powerColor;
				break;
			case Pickup.Type.Life:
				p.render.sprite = lifeSprite;
				p.render.color = lifeColor;
				break;
			case Pickup.Type.PointValue:
				p.render.sprite = pointValueSprite;
				p.render.color = pointValueColor;
				break;
			}
			return p;
		}
		
		public static void AutoCollectAll()
		{
			foreach(Pickup pickup in All)
			{
				pickup.state = Pickup.State.AutoCollect;
				pickup.currentVelocity = -1f;
			}
		}
	}
}
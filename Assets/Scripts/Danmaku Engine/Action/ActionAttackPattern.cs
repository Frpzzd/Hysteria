using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Core;
using UnityEditor;

namespace DanmakuEngine.Actions
{
	[Serializable]
	public class ActionAttackPattern : AbstractAttackPattern
	{
		[SerializeField]
		public MovementAction[] actions;
		
		[SerializeField]
		public FireTag[] fireTags;
		
		[SerializeField]
		public BulletTag[] bulletTags;
		
		public ActionAttackPattern()
		{
			fireTags = new FireTag[1];
			fireTags [0] = new FireTag ();
			bulletTags = new BulletTag[1];
			bulletTags [0] = new BulletTag ();
			actions = new MovementAction[1];
			actions [0] = new MovementAction ();
		}
		
		public override IEnumerator Run()
		{
			bulletsInPlay = new List<Bullet> ();
			currentHealth = health;
			parent.StartCoroutine (Move ());
			if(parent is Boss)
			{
				parent.StartCoroutine (Timer ());
			}
			for(int i = 0; i < fireTags.Length; i++)
			{
				if(fireTags[i].runAtStart)
				{
					parent.StartCoroutine(fireTags[i].Run(parent, this));
				}
			}
			while(currentHealth > 0)
			{
				yield return new WaitForFixedUpdate();
			}
			if(parent is Boss)
			{
				foreach(Bullet bullet in bulletsInPlay.ToArray())
				{
					if(bullet.rend.isVisible)
					{
						bullet.Cancel();
					}
					else
					{
						bulletsInPlay.Remove(bullet);
					}
				}
			}
			else
			{
				bulletsInPlay.Clear();
			}
		}
		
		public void Stop()
		{
			Debug.Log ("Stop Pattern");
			currentHealth = -100;
		}
		
		private IEnumerator Move()
		{
			IEnumerator pause, actionEnumerator;
			bool mirrorMovementX = (parent is Enemy) ? (parent as Enemy).mirrorMovementX : false;
			bool mirrorMovementY = (parent is Enemy) ? (parent as Enemy).mirrorMovementY : false;
			foreach(MovementAction action in actions)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				actionEnumerator = action.Execute(parent.transform, this, mirrorMovementX, mirrorMovementY);
				while(actionEnumerator.MoveNext())
				{
					yield return actionEnumerator.Current;
				}
			}
		}

		public override void DrawHandles (Vector3 spawnPosition, bool mirrorMoveX, bool mirrorMoveY, Color handlesColor)
		{
			Color oldColor = Handles.color;
			Handles.color = handlesColor;
			Vector3 endLocation = spawnPosition;
			for(int i = 0; i < actions.Length; i++)
			{
				endLocation = actions[i].DrawHandles(endLocation, mirrorMoveX, mirrorMoveY, Color.yellow);
			}
			Handles.color = oldColor;
		}
		
		private IEnumerator Timer()
		{
			float deltat = Time.fixedDeltaTime;
			remainingTime = timeout;
			remainingBonus = bonus;
			success = true;
			IEnumerator pause;
			while(remainingTime > 0)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				if(currentHealth <= 0)
				{
					return false;
				}
				yield return new WaitForFixedUpdate();
				remainingTime -= deltat;
				remainingBonus = (success) ? (int)Mathf.Lerp((float)bonus, 0f, 1f - (remainingTime/timeout)) : 0;
			}
			Stop ();
		}
		
		public override void Initialize(AbstractEnemy parent)
		{
			this.parent = parent;
			foreach(MovementAction action in actions)
			{
				action.Initialize(parent);
			}
			foreach(FireTag tag in fireTags)
			{
				tag.Initialize(parent);
			}
			foreach(BulletTag tag in bulletTags)
			{
				tag.Initialize(parent);
			}
		}

		public void Fire<T, P>(T action, AbstractEnemy master, Vector3 position, Quaternion rotation, float param, SequenceWrapper sequence)
			where T : AttackPatternAction<T, P>
			where P : struct, IConvertible
		{
			if(!parent.renderer.isVisible)
			{
				return;
			}
			Vector3 sourcePos = Vector3.zero;
			BulletTag bt = bulletTags[action.bulletTagIndex];

			sequence.sourceRadius = action.sourceRadius.Value + ((action.sourceRadius.sequence) ? sequence.sourceRadius : 0f);
			sequence.sourceTheta = action.sourceRadius.Value + ((action.sourceTheta.sequence) ? sequence.sourceTheta : 0f);
			
			sourcePos = GetSourcePosition<T, P>(position, action, sequence) + new Vector3(Mathf.Cos(sequence.sourceTheta), Mathf.Sin(sequence.sourceTheta)) * sequence.sourceRadius;

			sequence.angle = (action.useParam) ? param : action.angle.Value + ((action.angle.sequence) ? sequence.angle : 0f);
			sequence.speed = (action.overwriteBulletSpeed) ? action.speed.Value + ((action.speed.sequence) ? sequence.speed : 0f) : bt.speed.Value;

			float baseAngle = 0f;
			switch(action.Direction)
			{	
				case (DirectionType.Absolute):
					baseAngle = 0f;
					break;
					
				case (DirectionType.Relative):
					baseAngle = rotation.eulerAngles.z;
					break;

				case (DirectionType.TargetPlayer):
					Vector3 r = Player.PlayerTransform.position - sourcePos;
					baseAngle = Mathf.Atan2(r.y, r.x) * (180f/Mathf.PI) - 90;
					break;
			}

			Fire (BulletPool.Get (bt), sourcePos, baseAngle + sequence.angle, sequence.speed, action.fake);
			if(action.mirrorFire)
			{
				Fire (BulletPool.Get (bt), sourcePos, baseAngle - sequence.angle, sequence.speed, action.fake);
			}
			//		if(action.passParam)
			//		{
			//			temp.param = UnityEngine.Random.Range(action.paramRange.x, action.paramRange.y);
			//		}
			//		
			//		if(action.passPassedParam)
			//		{
			//			temp.param = param;
			//		}
			SoundManager.PlaySoundEffect (action.audioClip, sourcePos);
		}
		
		public static Vector3 GetSourcePosition<T, P>(Vector3 currentPosition, T action, SequenceWrapper sequence)
			where T : AttackPatternAction<T, P>
				where P : struct, IConvertible
		{
			Vector2 center = Vector2.zero;
			switch(action.source)
			{
			case SourceType.Attacker:
				center = currentPosition;
				break;
			case SourceType.Absolute:
				center = action.location;
				break;
			case SourceType.Relative:
				center = new Vector2(currentPosition.x, currentPosition.y) + action.location;
				break;
			case SourceType.Sequence:
				center = new Vector2(sequence.sourceOrigin.x, sequence.sourceOrigin.y) + action.location;
				break;
			case SourceType.Player:
				center = new Vector2(Player.Instance.Transform.position.x, Player.Instance.Transform.position.y) + action.location;
				break;
			default:
				return Vector3.zero;
			}
			sequence.sourceOrigin = GetRandomPosition (center, action.randomArea, currentPosition.z, action.randomStyle);
			return sequence.sourceOrigin;
		}
		
		private static Vector3 GetRandomPosition(Vector2 start, Vector2 size, float z, RandomStyle randomStyle)
		{
			if(randomStyle == RandomStyle.Rectangular)
			{
				return new Vector3(start.x - 0.5f * size.x + size.x * UnityEngine.Random.value, start.y - 0.5f * size.y + size.y * UnityEngine.Random.value, z);
			}
			else if(randomStyle == RandomStyle.Elliptical)
			{
				float theta = 2 * Mathf.PI * UnityEngine.Random.value;
				float r = UnityEngine.Random.value;
				return new Vector3(start.x + size.x * r * Mathf.Cos(theta), start.y + size.y * r * Mathf.Sin(theta), z);
			}
			else
			{
				return new Vector3(start.x, start.y, z);
			}
		}
	}

	public enum DirectionType { Absolute, Relative, TargetPlayer}
	public enum SourceType { Attacker, Absolute, Relative, Sequence, Player }
	public enum RandomStyle { None, Rectangular, Elliptical }
	public enum SelectionType { None, Movement, Fire, Bullet }
	
	[Serializable]
	public abstract class AttackPatternAction<T, P> : NestedAction<T, P> 
		where T : AttackPatternAction<T, P>
		where P : struct, IConvertible
	{	
		[DanmakuProperty("Angle Offset", false, true)]
		public DanmakuProperty angle;
		[DanmakuProperty("Speed", false, true)]
		public DanmakuProperty speed;
		public DirectionType direction;
		public bool waitForFinish = false;
		public bool mirrorFire = false;
		
		public SourceType source;
		public RandomStyle randomStyle = RandomStyle.None;
		public Vector2 randomArea = Vector2.zero;
		public Vector2 location = Vector3.zero;
		[DanmakuProperty("Source Offset Radius", false, true)]
		public DanmakuProperty sourceRadius;
		[DanmakuProperty("Source Offset Theta", false, true)]
		public DanmakuProperty sourceTheta;
		
		public int bulletTagIndex;
		
		public bool useParam = false;
		public bool overwriteBulletSpeed = false;
		public bool useSequenceSpeed = false;
		
		//	public bool passParam = false;
		//	public bool passPassedParam = false;
		//	public Vector2 paramRange;
		
		public DirectionType Direction;
		
		public AudioClip audioClip = null;
		
		public bool fake;
	}
	
	public class SequenceWrapper
	{
		public Vector3 sourceOrigin = Vector3.zero;
		public float sourceRadius = 0;
		public float sourceTheta = 0;
		
		public float angle = 0;
		public float speed = 0;
	}
}
 
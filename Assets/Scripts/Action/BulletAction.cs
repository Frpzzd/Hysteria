using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IBulletAction : Action
{
}

public abstract class BulletAction : AttackPatternAction, IBulletAction
{
	//	public enum Type { Wait, ChangeDirection, ChangeSpeed, ChangeScale, Repeat, Fire, VerticalChangeSpeed, Deactivate }
	//	public bool waitForChange = false;
	public bool WaitForChange;
	public AttackPattern.Property wait;

	public BulletAction()
	{
	}

	public class ChangeDirection : BulletAction
	{
		public override ActionType Type{ get { return (WaitForChange) ? ActionType.Coroutine : ActionType.Normal; } }
		public AttackPattern.Property angle;
		public DirectionType direction;
		
		#if UNITY_EDITOR
		public override void ActionGUI (AttackPattern master)
		{
			direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", direction);
			angle.EditorGUI("Angle", false);
			wait.EditorGUI("Time", false);
			WaitForChange = EditorGUILayout.Toggle("Wait To Finish", WaitForChange);
		}
		
		public override void DrawHandles ()
		{
			//No handles need to be drawn for Wait
		}
		#endif
		
		public override void Execute(params object[] param)
		{
			CoroutineExecute (param);
		}
		
		public override YieldInstruction YieldExecute(params object[] param)
		{
			throw new InvalidOperationException();
		}
		
		public override IEnumerator CoroutineExecute(params object[] param)
		{
			Bullet bullet = param [0] as Bullet;

			float t = 0.0f, d, ang;
			int dir;
			Quaternion newRot = Quaternion.identity;
			
			d = wait.Value * Time.deltaTime;
			
			Quaternion originalRot = bullet.Transform.localRotation;
			
			// determine offset
			ang = angle.Value;
			
			//and set rotation depending on angle
			switch(direction)
			{
			case (DirectionType.TargetPlayer):
				float dotHeading = Vector3.Dot( bullet.Transform.up, Player.PlayerTransform.position - bullet.Transform.position );		
				if(dotHeading > 0)
					dir = -1;
				else
					dir = 1;
				float angleDif = Vector3.Angle(bullet.Transform.forward, Player.PlayerTransform.position - bullet.Transform.position);
				newRot = originalRot * Quaternion.AngleAxis((dir * angleDif) - ang, Vector3.right); 
				break;
				
			case (DirectionType.Absolute):
				newRot = Quaternion.Euler(-(ang - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				newRot = originalRot * Quaternion.AngleAxis(-ang, Vector3.right);
				break;
			}
			
			//Sequence has its own thing going on, continually turning a set amount until time is up
			if(direction == DirectionType.Sequence)
			{
				newRot = Quaternion.AngleAxis (-ang, Vector3.right); 
				
				while(t < d)
				{
					bullet.Transform.localRotation *= newRot;
					t += Time.deltaTime;
					yield return new WaitForFixedUpdate();
				}
			}
			//all the others just linearly progress to destination rotation
			else if(d > 0)
			{
				while(t < d)
				{
					bullet.Transform.localRotation = Quaternion.Slerp(originalRot, newRot, t/d);
					t += Time.deltaTime;
					yield return new WaitForFixedUpdate();
				}
				
				bullet.Transform.localRotation = newRot;
			}
		}
	}
	
	public class ChangeSpeed : BulletAction
	{
		public AttackPattern.Property speed;

		public bool isVertical;
		
		public override ActionType Type{ get { return (WaitForChange) ? ActionType.Coroutine : ActionType.Normal; } }
		
		#if UNITY_EDITOR
		public override void ActionGUI (AttackPattern master)
		{
	        speed.EditorGUI("Speed", false);
	        wait.EditorGUI("Time", false);
	        WaitForChange = EditorGUILayout.Toggle("Wait To Finish", WaitForChange);
		}
		
		public override void DrawHandles ()
		{
			//No handles need to be drawn for Wait
		}
		#endif
		
		public override void Execute (params object[] param)
		{
			CoroutineExecute (param);
		}
		
		public override YieldInstruction YieldExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator CoroutineExecute(params object[] param)
		{
			Bullet bullet = param [0] as Bullet;

			float t = 0.0f;
			float s = 0.0f;
			float d, newSpeed;
			
			if(isVertical)
			{
				bullet.useVertical = true;
			}
			
			d = wait.Value * Time.deltaTime;	
			
			float originalSpeed = bullet.speed;
			
			newSpeed = speed.UnrankedValue;
			if(speed.rank)
				d += speed.RankValue;
			
			if(d > 0)
			{
				while(t < d)
				{
					s = Mathf.Lerp(originalSpeed, newSpeed, t/d);
					if(isVertical) 
					{
						bullet.verticalSpeed = s;
					}
					else 
					{
						bullet.speed = s;
					}
					t += Time.deltaTime;
					yield return new WaitForFixedUpdate();
				}
			}
			
			if(isVertical)
			{
				bullet.verticalSpeed = newSpeed;
			}
			else 
			{
				bullet.speed = newSpeed;
			}
		}
	}

	public class Repeat : SharedAction.Repeat<IBulletAction, SharedAction.Wait>, IBulletAction
	{
		//Copy of SharedAction.Repeat for Bullet Actions
	}
}
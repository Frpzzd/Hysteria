using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class BulletAction : AttackPatternAction<BulletAction, BulletAction.Type>
{
	public enum Type { Wait, ChangeDirection, ChangeSpeed, ChangeScale, Repeat, Fire, Deactivate }
	public override ActionType ActionType 
	{
		get
		{
			switch(type)
			{
				case Type.ChangeDirection:
				case Type.ChangeScale:
				case Type.ChangeSpeed:
					return (waitForFinish) ? ActionType.Normal : ActionType.Coroutine;
				default:
					return ActionType.Normal;
			}
		}
	}
	[SerializeField]
	public bool isVertical;
	
	#if UNITY_EDITOR
	public override void ActionGUI(params object[] param)
	{
		type = (Type)EditorGUILayout.EnumPopup("Type", type);
		AttackPattern attackPattern = param [0] as AttackPattern;
		switch(type)
		{
			case Type.ChangeDirection:
				direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", direction);
				angle = AttackPattern.Property.EditorGUI("Angle", angle, false);
				wait = AttackPattern.Property.EditorGUI("Time", wait, false);
				waitForFinish = EditorGUILayout.Toggle("Wait To Finish", waitForFinish);
				break;
			case Type.ChangeSpeed:
				speed = AttackPattern.Property.EditorGUI("Speed", speed, false);
				wait = AttackPattern.Property.EditorGUI("Time", wait, false);
				waitForFinish = EditorGUILayout.Toggle("Wait To Finish", waitForFinish);
				break;
			case Type.Wait:
				wait = AttackPattern.Property.EditorGUI ("Wait", wait, false);
				break;
			case Type.Repeat:
				SharedAction.Repeat.ActionGUI<BulletAction, BulletAction.Type> (this, param);
				break;
			case Type.Fire:
				SharedAction.Fire.ActionGUI<BulletAction, BulletAction.Type>(this, attackPattern);
				break;
		}
	}

	public override void DrawGizmosImpl (BulletAction previous)
	{

	}
	#endif

	public override IEnumerator Execute(params object[] param)
	{
		Bullet bullet = param [0] as Bullet;
		AttackPattern attackPattern = param [1] as AttackPattern;
		float t = 0.0f;
		float s = 0.0f;
		float d, newSpeed, ang;
		switch(type)
		{
			case Type.ChangeDirection:
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
				break;
			case Type.ChangeSpeed:
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
				break;
			case Type.Repeat:
				yield return parent.StartCoroutine(ActionHandler.ExecuteActions(nestedActions, param));
				break;
			case Type.Wait:
				yield return new WaitForSeconds(wait.Value);
				break;
			case Type.Fire:
				SharedAction.Fire.Execute<BulletAction, BulletAction.Type>(this, parent as Enemy, attackPattern, 
				                                                       parent.transform.position, 
				                                                       parent.transform.rotation, 
				                                                       bullet.param, 
				                                                       bullet.prevRotation);
				break;
		}
	}
}
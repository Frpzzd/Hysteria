using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class SharedAction //: AbstractAction, IFireAction, IBulletAction, IMovementAction, IStageAction
{
	public static class Repeat
	{
		#if UNITY_EDITOR
		public static void ActionGUI<T, P>(T nestedAction, MonoBehaviour parent, params object[] param) 
			where T : NestedAction<T, P>, new() 
			where P : struct, IConvertible
		{
			if(nestedAction != null)
			{
				if (nestedAction.nestedActions == null || nestedAction.nestedActions.Length == 0)
				{
					nestedAction.nestedActions = new T[1];
					nestedAction.nestedActions [0] = new T();
				}

				nestedAction.repeat = AttackPattern.Property.EditorGUI("Repeat", nestedAction.repeat, true);
				
				nestedAction.nestedActions = EditorUtils.ActionGUI<T, P>(nestedAction.nestedActions, false, parent, param);
			}
		}

		public static void DrawGizmos<T, P>(T nestedAction) where T : NestedAction<T, P> where P : struct, IConvertible
		{
			if(nestedAction.nestedActions != null)
			{
				for(int j = 0; j < Mathf.FloorToInt(nestedAction.repeat.Value); j++)
				{
					for(int i = 0; i < nestedAction.nestedActions.Length; i++)
					{
						if(i == 0)
						{
							if(j == 0)
							{
								nestedAction.nestedActions[i].DrawGizmos(null);
							}
							else
							{
								nestedAction.nestedActions[i].DrawGizmos(nestedAction.nestedActions[nestedAction.nestedActions.Length - 1]);
							}
						}
						else
						{
							nestedAction.nestedActions[i].DrawGizmos(nestedAction.nestedActions[i - 1]);
						}
					}
				}
			}
		}
		#endif

		public static IEnumerator Execute<T, P>(T[] nestedActions, AttackPattern.Property repeat, params object[] param) where T : NestedAction<T, P> where P : struct, IConvertible
		{		
			if(nestedActions != null && nestedActions.Length > 0)
			{
				for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
				{
					yield return nestedActions[0].parent.StartCoroutine(ActionHandler.ExecuteActions(nestedActions, param));
				}
			}
			else
			{
				Debug.Log("Null or Empty Repeat call");
			}
		}
	}

	public static class Fire
	{
		#if UNITY_EDITOR
		public static void ActionGUI<T, P> (AttackPatternAction<T, P> action, AttackPattern master) 
			where T : NestedAction<T, P>
			where P : struct, IConvertible
		{
			action.source = (SourceType)EditorGUILayout.EnumPopup ("Source", action.source);
			switch(action.source)
			{
				case SourceType.Attacker:
					break;
				case SourceType.AnotherObject:
				action.alternateSource = (Transform)EditorGUILayout.ObjectField("Alternate Source", action.alternateSource, typeof(Transform), true);
					break;
				case SourceType.Absolute:
				case SourceType.Relative:
					action.location = EditorGUILayout.Vector2Field("Location", action.location);
					action.randomStyle = (RandomStyle)EditorGUILayout.EnumPopup("Randomness", action.randomStyle);
					if(action.randomStyle !=  RandomStyle.None)
					{
						action.randomArea = EditorGUILayout.Vector2Field("Random Area", action.randomArea);
					}
					break;
			}
			
			action.Direction = (DirectionType)EditorGUILayout.EnumPopup("Direction", action.Direction);
			if (!action.useParam)
			{
				action.angle = AttackPattern.Property.EditorGUI("Angle", action.angle, false);
			}
			action.useParam = EditorGUILayout.Toggle("Use Param Angle", action.useParam);
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			action.overwriteBulletSpeed = EditorGUILayout.Toggle("Overwrite Speed", action.overwriteBulletSpeed);
			if(action.overwriteBulletSpeed)
			{
				action.useSequenceSpeed = EditorGUILayout.Toggle("Use Sequence Speed", action.useSequenceSpeed);
			}
			EditorGUILayout.EndHorizontal();
			if(action.overwriteBulletSpeed && !action.useSequenceSpeed)
			{
				action.speed = AttackPattern.Property.EditorGUI("Speed", action.speed, false);
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			action.passParam = EditorGUILayout.Toggle("PassParam", action.passParam);
			if (!action.passParam)
			{
				action.passPassedParam = EditorGUILayout.Toggle("PassMyParam", action.passPassedParam);
			}
			EditorGUILayout.EndHorizontal();    
			if (action.passParam)
			{
				action.paramRange = EditorGUILayout.Vector2Field("Param Range", action.paramRange);
			}
			action.bulletTagIndex = EditorUtils.NamedObjectPopup("Bullet Tag", master.bulletTags, action.bulletTagIndex, "Bullet Tag");
			action.bulletTag = master.bulletTags [action.bulletTagIndex];
		}
		#endif

		public static void Execute<T, P>(T action, AttackPattern master, params object[] parameters) 
			where T : AttackPatternAction<T, P>
			where P : struct, IConvertible
		{
			Vector3 position;
			Quaternion rotation;
			RotationWrapper previousRotation;
			float param;
			if(parameters[0] is FireTag)
			{
				FireTag tag = (parameters[0] as FireTag);
				position = GetSourcePosition<T, P>(master.Transform.position, action);
				rotation = master.Transform.rotation;
				param = tag.param;
				previousRotation = tag.previousRotation;
			}
			else
			{
				Bullet bullet = (parameters[0] as Bullet);
				position = GetSourcePosition<T, P>(bullet.Transform.position, action);
				rotation = bullet.Transform.rotation;
				param = bullet.param;
				previousRotation = bullet.prevRotation;
			}
			
			
			float angle, direction, angleDifference, speed;
			BulletTag bt = action.bulletTag;
			Bullet temp = GameObjectManager.Bullets.Get(bt);
			if(previousRotation.rotationNull)
			{
				previousRotation.rotationNull = false;
				previousRotation.rotation = temp.Transform.localRotation;
			}
			
			temp.Transform.position = position;
			temp.Transform.rotation = rotation;
			
			if(action.useParam)
			{
				angle = param;
			}
			else
			{
				angle = action.angle.Value;
			}
			
			switch(action.Direction)
			{
			case (DirectionType.TargetPlayer):
				Quaternion originalRot = rotation;
				float dotHeading = Vector3.Dot( temp.Transform.up, Player.PlayerTransform.position - temp.Transform.position );
				
				if(dotHeading > 0)
				{
					direction = -1;
				}
				else
				{
					direction = 1;
				}
				angleDifference = Vector3.Angle(temp.Transform.forward, Player.PlayerTransform.position - temp.Transform.position);
				temp.Transform.rotation = originalRot * Quaternion.AngleAxis((direction * angleDifference) - angle, Vector3.right);
				break;
				
			case (DirectionType.Absolute):
				temp.Transform.localRotation = Quaternion.Euler(-(angle - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				temp.Transform.localRotation = rotation * Quaternion.AngleAxis (-angle, Vector3.right);
				break;
				
			case (DirectionType.Sequence):
				temp.Transform.localRotation = previousRotation.rotation * Quaternion.AngleAxis (-angle, Vector3.right); 
				break;
			}
			previousRotation.rotation = temp.Transform.localRotation;
			if(action.overwriteBulletSpeed)
			{
				speed = action.speed.Value;
				
				if(action.useSequenceSpeed)
				{
					master.sequenceSpeed += speed;
					temp.speed = master.sequenceSpeed;
				}
				else
				{
					master.sequenceSpeed = 0.0f;
					temp.speed = speed;
				}
			}
			else
			{	
				temp.speed = bt.speed.Value;
			}
			
			if(action.passParam)
			{
				temp.param = UnityEngine.Random.Range(action.paramRange.x, action.paramRange.y);
			}
			
			if(action.passPassedParam)
			{
				temp.param = param;
			}
			temp.master = master;
			temp.GameObject.SetActive(true);
			SoundManager.PlaySoundEffect (action.audioClip, position);
		}

		public static Vector3 GetSourcePosition<T, P>(Vector3 currentPosition, T action) 
			where T : AttackPatternAction<T, P>
			where P : struct, IConvertible
		{
			switch(action.source)
			{
				case SourceType.Attacker:
					return currentPosition;
				case SourceType.Absolute:
					return GetRandomPosition(action.location, action.randomArea, currentPosition.z, action.randomStyle);
				case SourceType.Relative:
					return GetRandomPosition(new Vector2(currentPosition.x, currentPosition.y) + action.location, action.randomArea, currentPosition.z, action.randomStyle);
				case SourceType.AnotherObject:
					return action.alternateSource.position;
				default:
					return Vector3.zero;
			}
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
}
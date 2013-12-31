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
		public static void ActionGUI<T, P>(T nestedAction, params object[] param) 
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
				
				nestedAction.nestedActions = EditorUtils.ActionGUI<T, P>(nestedAction.nestedActions, false, param);
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

//		public static IEnumerator Execute<T, P>(T[] nestedActions, AttackPattern.Property repeat, params object[] param) where T : NestedAction<T, P> where P : struct, IConvertible
//		{		
//			if(nestedActions != null && nestedActions.Length > 0)
//			{
//				for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
//				{
//					yield return nestedActions[0].parent.StartCoroutine(ActionHandler.ExecuteActions(nestedActions, param));
//				}
//			}
//			else
//			{
//				Debug.Log("Null or Empty Repeat call");
//			}
//		}
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
		}
		#endif
	}
}
using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class MovementAction : NestedAction<MovementAction, MovementAction.Type>
{
	public enum Type { Wait, Repeat, Linear, Teleport }
	public enum LocationType { Absolute, Relative }
	
	[SerializeField]
	public Vector3 targetLocation;
	[SerializeField]
	public LocationType locationType;

	#if UNITY_EDITOR
	public override void ActionGUI(params object[] param)
	{
		type = (Type)EditorGUILayout.EnumPopup("Type", type);
		if(type != Type.Wait  && type != Type.Repeat)
		{
			locationType = (LocationType)EditorGUILayout.EnumPopup("Loaction Type", locationType);
			targetLocation = EditorGUILayout.Vector2Field("Target", targetLocation);
		}
		switch(type)
		{
			case Type.Wait:
				wait = AttackPattern.Property.EditorGUI ("Wait", wait, false);
				break;
			case Type.Repeat:
				SharedAction.Repeat.ActionGUI<MovementAction, MovementAction.Type>(this, parent);
				break;
			case Type.Linear:
				wait = AttackPattern.Property.EditorGUI ("Total Time", wait, false);
				break;
		}
	}

	public override void DrawGizmosImpl (MovementAction previous)
	{

	}

	public Vector3 DrawGizmos(Vector3 previousPosition, Color gizmoColor)
	{
		Vector3 actualLocation = previousPosition;
		if(type != Type.Wait)
		{
			if(type == Type.Repeat)
			{
				for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
				{
					Vector3 temp = actualLocation;

					for(int i = 0; i < nestedActions.Length; i++)
					{
						actualLocation = nestedActions[i].DrawGizmos(actualLocation, gizmoColor);
					}

					if(temp == actualLocation)
					{
						break; //Loop doesn't change, just draw once
					}
				}
			}
			else 
			{
				if(locationType == LocationType.Relative)
				{
					actualLocation = previousPosition + targetLocation;
				}
				else
				{
					actualLocation = targetLocation;
				}
				Gizmos.DrawLine(previousPosition, actualLocation);
				Gizmos.DrawWireSphere(actualLocation, 1);
			}
		}
		return actualLocation;
	}
	#endif

	public override IEnumerator Execute (params object[] param)
	{
		Transform transform = param [0] as Transform;
		AttackPattern attackPattern = param [1] as AttackPattern;
		if(attackPattern.currentHealth < 0)
		{
			return false;
		}
		Vector3 start = Vector3.zero, end = Vector3.zero;
		float totalTime = wait.Value;
		float deltat = Time.fixedDeltaTime;
		switch(type)
		{
			case Type.Linear:
			case Type.Teleport:
				start = transform.position;
				end = targetLocation + ((locationType == LocationType.Relative) ? start : Vector3.zero);
				break;
		}
		switch(type)
		{
			case Type.Linear:
				float lerpValue = 0f;
				while(lerpValue <= 1f)
				{
					yield return Global.WaitForUnpause();
					transform.position = Vector3.Lerp(start, end, lerpValue);
					lerpValue +=  deltat / totalTime;
					yield return new WaitForFixedUpdate();
					//Figure out spline movement
				}
				transform.position = end;
				break;
			case Type.Teleport:
				//TODO: Play open teleport effect here
				(param[0] as Transform).position = end;
				//TODO: Play close teleport effect here
				break;
			case Type.Repeat:
				int repeatC = Mathf.FloorToInt(repeat.Value);
				for(int j = 0; j < repeatC; j++)
				{
					foreach(Action action in nestedActions)
					{
						if(attackPattern.currentHealth < 0)
						{
							return false;
						}
						yield return action.Execute(param[0], param[1]);
					}
				}
				break;
			case Type.Wait:
				float currentTime = 0f;
				while(currentTime < totalTime)
				{
					yield return Global.WaitForUnpause();
					if(attackPattern.currentHealth <= 0)
					{
						return false;
					}
					yield return new WaitForFixedUpdate();
					totalTime += Time.fixedDeltaTime;
				}
				break;
		}
	}
}

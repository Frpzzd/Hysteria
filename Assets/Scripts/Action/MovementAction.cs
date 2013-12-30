using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovementAction : NestedAction<MovementAction, MovementAction.Type>
{
	public override ActionType ActionType { get { return ActionType.Normal; } }
	public enum Type { Wait, Repeat, Absolute, Relative, Teleport }
	public Vector3 targetLocation;

	protected static IEnumerator LinearMove(Transform transform, Vector3 start, Vector3 end, float totalTime)
	{
		float lerpValue = 0f;
		while(lerpValue <= 1f)
		{
			transform.position = Vector3.Lerp(start, end, lerpValue);
			lerpValue += Time.fixedDeltaTime / totalTime;
			yield return new WaitForFixedUpdate();
			//Figure out 
		}
	}

	#if UNITY_EDITOR
	protected override void ActionGUIImpl (MonoBehaviour parent, params object[] param)
	{
		type = (Type)EditorGUILayout.EnumPopup("Type", type);
		if(type != Type.Wait  && type != Type.Repeat)
		{
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
			case Type.Absolute:
			case Type.Relative:
				wait = AttackPattern.Property.EditorGUI ("Total Time", wait, false);
				break;
		}
	}

	public override void DrawGizmosImpl (MovementAction previous)
	{
		
	}
	#endif

	public override IEnumerator Execute (params object[] param)
	{
		Transform transform = param [0] as Transform;
		switch(type)
		{
			case Type.Absolute:
				//TODO: Spline interpolation
				LinearMove (transform, transform.position, targetLocation, wait.Value);
				break;
			case Type.Relative:
				//TODO: Spline interpolation
				LinearMove (transform, transform.position, transform.position + targetLocation, wait.Value);
				break;
			case Type.Teleport:
				//TODO: Play open teleport effect here
				(param[0] as Transform).position = targetLocation;
				//TODO: Play close teleport effect here
				break;
			case Type.Repeat:
				SharedAction.Repeat.Execute<MovementAction, MovementAction.Type>(nestedActions, repeat, param);
				break;
			case Type.Wait:
				yield return new WaitForSeconds(wait.Value);
				break;
		}
	}
}

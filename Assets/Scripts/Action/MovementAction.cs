using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IMovementAction : Action
{
}

public abstract class MovementAction : AbstractAction, IMovementAction
{
	public Vector3 targetLocation;
	public override ActionType Type { get { return ActionType.Normal; } }

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
	public override void ActionGUI (params object[] param)
	{

	}

	public override void DrawHandles()
	{

	}
	#endif

	public class Absolute : MovementAction
	{
		public float totalTime;

		public override void Execute (params object[] param)
		{
			//TODO: Spline interpolation
			Transform transform = param [0] as Transform;
			LinearMove (transform, transform.position, targetLocation, totalTime);
		}

		public override YieldInstruction YieldExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}

		public override IEnumerator CoroutineExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
	}

	public class Relative : MovementAction
	{
		public float totalTime;

		public override void Execute (params object[] param)
		{
			//TODO: Spline interpolation
			Transform transform = param [0] as Transform;
			LinearMove (transform, transform.position, transform.position + targetLocation, totalTime);
		}
		
		public override YieldInstruction YieldExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator CoroutineExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
	}

	public class Teleport : MovementAction
	{
		public override void Execute (params object[] param)
		{
			//TODO: Play open teleport effect here
			(param[0] as Transform).position = targetLocation;
			//TODO: Play close teleport effect here
		}
		
		public override YieldInstruction YieldExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator CoroutineExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
	}

	public class Repeat : SharedAction.Repeat<IMovementAction, SharedAction.Wait>, IMovementAction
	{
		//Copy of Repeat for Movement Actions
	}
}

using UnityEngine;
using System;
using System.Collections;

public interface IFireAction : Action
{
	void Execute(FireTag tag);
	YieldInstruction YieldExecute(FireTag tag);
	IEnumerator Coroutine (FireTag tag);
}

public abstract class FireAction : AbstractAction, IFireAction
{
	//	public enum Type { Wait, Fire, CallFireTag, Repeat }
	public abstract void Execute(FireTag tag);
	public abstract YieldInstruction YieldExecute(FireTag tag);
	public abstract IEnumerator Coroutine(FireTag tag);

	public class Repeat : FireAction, INestedAction
	{
		public override ActionType Type { get { return ActionType.Normal; } }
		public AttackPattern.Property repeat;
		public IFireAction[] nestedActions;
		
		#if UNITY_EDITOR
		public override void ActionGUI (AttackPattern master)
		{
			this.master = master;
			if (nestedActions == null || nestedActions.Length == 0)
			{
				nestedActions = new IFireAction[1];
				nestedActions [0] = new SharedAction.Wait();
			}

			repeat.EditorGUI ("Repeat Count", true);

			nestedActions = EditorUtils.FireActionGUI (nestedActions, master);
		}
		
		public override void DrawHandles ()
		{
			//TO-DO
		}
		
		public void Expand(bool recursive)
		{
			SetAll (true, recursive);
		}
		
		public void Collapse(bool recursive)
		{
			SetAll (false, recursive);
		}
		
		public void SetAll(bool value, bool recursive)
		{
			Foldout = value;
			if(recursive && nestedActions != null && nestedActions.Length > 0)
			{
				for(int i = 0; i < nestedActions.Length; i++)
				{
					if(nestedActions[i] is INestedAction)
					{
						(nestedActions[i] as INestedAction).SetAll(value, recursive);
					}
				}
			}
		}
		#endif
		public override void Execute (FireTag tag)
		{
			Coroutine (tag);
		}
		
		public override IEnumerator Coroutine (FireTag tag)
		{
			for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
			{
				for(int i = 0; i < nestedActions.Length; i++)
				{
					switch(nestedActions[i].Type)
					{
						case ActionType.Normal:
							nestedActions[i].Execute(tag);
							break;
						case ActionType.Yield:
							yield return nestedActions[i].YieldExecute(tag);
							break;
						case ActionType.Coroutine:
							yield return master.StartCoroutine(nestedActions[i].Coroutine(tag));
							break;
					}
				}
			}
		}
		
		public override YieldInstruction YieldExecute (FireTag tag)
		{
			throw new NotImplementedException ();
		}
	}
}

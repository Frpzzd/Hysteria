using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ActionType { Normal, Yield, Coroutine }

public interface Action
{
	ActionType Type { get; }
	MonoBehaviour Parent { get; set; }

#if UNITY_EDITOR
	bool Foldout { get; set; }

	void ActionGUI(params object[] param);
	void DrawHandles();
#endif

	void Execute(params object[] param);
	YieldInstruction YieldExecute(params object[] param);
	IEnumerator CoroutineExecute(params object[] param);
}

public static class ActionExecutor
{
	public static IEnumerator ExecuteAction(Action action, params object[] param)
	{
		switch(action.Type)
		{
			case ActionType.Normal:
				action.Execute (param);
				break;
			case ActionType.Yield:
				yield return action.YieldExecute (param);
				break;
			case ActionType.Coroutine:
				yield return action.Parent.StartCoroutine(action.CoroutineExecute (param));
				break;
		}
	}
}

public abstract class AbstractAction : Action
{
	public abstract ActionType Type{ get; }

	#if UNITY_EDITOR
	private bool foldout = true;
	public bool Foldout
	{
		get { return foldout; }
		set { foldout = value; }
	}
	public abstract void ActionGUI(params object[] param);
	public abstract void DrawHandles();
	#endif

	private MonoBehaviour parent;
	public MonoBehaviour Parent
	{
		get { return parent; }
		set { parent = value; }
	}
	
	public override string ToString ()
	{
		#if UNITY_EDITOR
		return EditorUtils.TypeDictionary.ProcessName(GetType());
		#else
		return base.ToString();
		#endif
	}

	public abstract void Execute(params object[] param);
	public abstract YieldInstruction YieldExecute(params object[] param);
	public abstract IEnumerator CoroutineExecute(params object[] param);
}

public interface INestedAction : Action
{
	#if UNITY_EDITOR
	void Expand (bool recursive);
	void Collapse(bool recursive);
	void SetAll (bool value, bool recursive);
	#endif
}

public abstract class NestedAction<T> : AbstractAction, INestedAction where T : Action
{
	protected T[] nestedActions;

	public T this[int index]
	{
		get
		{
			return (T)nestedActions[index];
		}

		set
		{
			nestedActions[index] = value;
		}
	}

	#if UNITY_EDITOR
	public abstract void Expand (bool recursive);
	public abstract void Collapse(bool recursive);
	public abstract void SetAll (bool value, bool recursive);
	#endif
}

public abstract class AttackPatternAction : AbstractAction
{
	protected AttackPattern master;

	public AttackPattern Master
	{
		get { return master; }
	}
	#if UNITY_EDITOR
	public override void ActionGUI (params object[] param)
	{
		ActionGUI ((AttackPattern)param [0]);
	}

	public abstract void ActionGUI(AttackPattern master);
	#endif
}

public abstract class AbstractActionGroup<T> where T : Action
{
	protected T[] actions;

	public T this[int index]
	{
		get
		{
			Debug.Log("Get");
			return actions[index];
		}

		set
		{
			Debug.Log("Set");
			actions[index] = value;
		}
	}

	public int Size
	{
		get { return actions.Length; }
	}

	public abstract void Run(params object[] param);
}
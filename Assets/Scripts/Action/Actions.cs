using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ActionHandler
{
	#if UNITY_EDITOR
	public static void DrawActionGizmo<T, P>(T action, T previous, Color gizmoColor) where T : NestedAction<T, P> where P : struct, IConvertible
	{
		action.DrawGizmos(previous);
	}

	public static void DrawActionGizmos<T, P>(T[] actions, Color groupColor) where T : NestedAction<T, P> where P : struct, IConvertible
	{
		if(actions != null)
		{
			for(int i = 0; i < actions.Length; i++)
			{
				actions[i].DrawGizmos((i == 0) ? null : actions[i - 1]);
			}
		}
	}
	#endif
}

[Serializable]
public abstract class Action
{
	[NonSerialized]
	public bool foldout = true;

	[NonSerialized]
	public bool Initialized = false;

	[SerializeField]
	public MonoBehaviour parent;

	[SerializeField]
	public AttackPattern.Property wait;

	public virtual void Initialize(MonoBehaviour parent)
	{
		this.parent = parent;
		Initialized = true;
	}

	#if UNITY_EDITOR
	public abstract void ActionGUI(params object[] param);
	#endif


	public abstract IEnumerator Execute(params object[] param);
}

[Serializable]
public abstract class NestedAction<T, P> : Action where T : NestedAction<T, P> where P : struct, IConvertible
{
	public T[] nestedActions;
	public P type;
	public AttackPattern.Property repeat;
	public bool hasWait = true;
	
	public bool drawGizmos = true;
	
	#if UNITY_EDITOR
	public void DrawGizmos(T previous)
	{
		if(drawGizmos)
		{
			DrawGizmosImpl(previous);
		}
	}
	
	public abstract void DrawGizmosImpl (T previous);

	public void Expand(bool recursive)
	{
		SetAll (true, recursive);
	}

	public override void Initialize (MonoBehaviour parent)
	{
		base.Initialize (parent);
		if(nestedActions != null)
		{
			foreach(T action in nestedActions)
			{
				action.Initialize(parent);
			}
		}
	}
	
	public void Collapse(bool recursive)
	{
		SetAll (false, recursive);
	}
	
	public void SetAll(bool value, bool recursive)
	{
		foldout = value;
		if(recursive && nestedActions != null && nestedActions.Length > 0)
		{
			for(int i = 0; i < nestedActions.Length; i++)
			{
				nestedActions[i].foldout = value;
				nestedActions[i].SetAll(value, recursive);
			}
		}
	}

	public void Trim(P repeatValue)
	{
		for(int i = 0; i < nestedActions.Length; i++)
		{
			if(nestedActions[i].type.Equals(repeatValue))
			{
				nestedActions[i].Trim(repeatValue);
			}
			else
			{
				nestedActions[i].nestedActions = null;
			}
		}
	}
	#endif

	public override string ToString ()
	{
		return type.ToString ();
	}
}

public interface IActionGroup
{
	#if UNITY_EDITOR
	void ActionGUI(params object[] param);
	void DrawGizmos(Color gizmoColor);
	#endif
	IEnumerator Run(params object[] param);
	void Initialize (MonoBehaviour parent);
}
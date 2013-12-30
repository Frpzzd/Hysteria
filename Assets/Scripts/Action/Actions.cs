using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public enum ActionType { Normal, Coroutine }

public static class ActionHandler
{
	public static IEnumerator ExecuteActions(Action[] actions, params object[] param)
	{
		for(int i = 0; i < actions.Length; i++)
		{
			yield return actions[i].parent.StartCoroutine(ExecuteAction(actions[i], param));
		}
	}

	public static IEnumerator ExecuteAction(Action action, params object[] param)
	{
		switch(action.ActionType)
		{
			case ActionType.Normal:
				yield return action.parent.StartCoroutine(action.Execute (param));
				break;
			case ActionType.Coroutine:
				action.parent.StartCoroutine(action.Execute (param));
				break;
		}
	}

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
	public abstract ActionType ActionType{ get; }

	public AttackPattern.Property wait;
	
	public bool foldout = true;

	#if UNITY_EDITOR
	public void ActionGUI(MonoBehaviour parent, params object[] param)
	{
		if(this.parent == null || this.parent != parent)
		{
			this.parent = parent;
			GUI.changed = true;
		}
		ActionGUIImpl (parent, param);
	}
	protected abstract void ActionGUIImpl(MonoBehaviour parent, params object[] param);
	#endif

	public MonoBehaviour parent;

	public abstract IEnumerator Execute(params object[] param);
}

[Serializable]
public abstract class NestedAction<T, P> : Action where T : NestedAction<T, P> where P : struct, IConvertible
{
	public T[] nestedActions;
	public P type;
	public AttackPattern.Property repeat;
	
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
}

[Serializable]
public abstract class AbstractActionGroup<T, P> : UnityEngine.Object, IActionGroup where T : NestedAction<T, P> where P : struct, IConvertible
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

	public virtual object[] AlternateParameters(params object[] param) { return param; }

	#if UNITY_EDITOR
	public abstract void ActionGUI(params object[] param);

	public void DrawGizmos(Color gizmoColor)
	{
		ActionHandler.DrawActionGizmos<T, P> (actions, gizmoColor);
	}
	#endif 
	public virtual IEnumerator Run(params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			yield return actions[0].parent.StartCoroutine (ActionHandler.ExecuteActions(actions, param));
		}
	}
}

public abstract class AbstractActionBehavior<T, P> : CachedObject, IActionGroup where T : NestedAction<T, P>, new() where P : struct, IConvertible
{
	public T[] actions;

	private bool running;
	
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

	public Color gizmoColor = Color.cyan;
	
	public int Size
	{
		get { return actions.Length; }
	}

	public abstract string ActionGUITitle { get; }

	public abstract object[] ActionParameters { get; }
	
	public virtual IEnumerator Run(params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			running = true;
			yield return StartCoroutine(ActionHandler.ExecuteActions(actions, param));
			running = false;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if(!running)
		{
			StartActions ();
		}
	}

	public void OnEnable()
	{
		if(!running)
		{
			StartActions ();
		}
	}

	public virtual void StartActions()
	{
		StartCoroutine(Run(ActionParameters));
	}
	
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = gizmoColor;
		DrawGizmos (gizmoColor);
	}

	public void ActionGUI (params object[] param)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new T[1];
			actions [0] = new T();
		}

		EditorUtils.ExpandCollapseButtons<T, P>(ActionGUITitle, actions);

		actions = EditorUtils.ActionGUI<T, P>(actions, false, this);
	}
	
	public void DrawGizmos(Color gizmoColor)
	{
		ActionHandler.DrawActionGizmos<T, P> (actions, gizmoColor);
	}
	#endif
}
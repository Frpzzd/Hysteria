using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public interface NamedObject
{
	string Name { get; set; }
}

public interface TitledObject
{
	string Title { get; set; }
}

//[Serializable]
//public abstract class Tag : IActionGroup, NamedObject
//{
//	#if UNITY_EDITOR
//	public abstract void ActionGUI(params object[] param);
//
//	public abstract void DrawGizmos(Color gizmoColor);
//	#endif
//
//	public abstract IEnumerator Run(params object[] param);
//
//	public abstract string Name { get; set; }
//}

[System.Serializable]
public class FireTag : IActionGroup, NamedObject
{
	[SerializeField]
	public string ftName = "Fire Tag";
	[SerializeField]
	public float param = 0.0f;
	[SerializeField]
	public RotationWrapper previousRotation = new RotationWrapper();
	[SerializeField]
	public FireAction[] actions;
	
	public string Name
	{
		get
		{
			return ftName;
		}
		
		set
		{
			ftName = value;
		}
	}

	public FireTag()
	{
		actions = new FireAction[1];
		actions [0] = new FireAction ();
	}
	
	public IEnumerator Run (params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			yield return actions[0].parent.StartCoroutine (ActionHandler.ExecuteActions(actions, this, param[0] as AttackPattern));
		}
	}

	public void Initialize(MonoBehaviour parent)
	{
		foreach(FireAction action in actions)
		{
			action.Initialize(parent);
		}
	}
	
	#if UNITY_EDITOR
	public void ActionGUI (params object[] param)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new FireAction[1];
			actions [0] = new FireAction();
		}
		
		EditorUtils.ExpandCollapseButtons<FireAction, FireAction.Type>("Fire Tag: " + Name, actions);
		
		actions = EditorUtils.ActionGUI<FireAction, FireAction.Type> (actions, false, param);
	}

	public void DrawGizmos(Color gizmoColor)
	{

	}
	#endif
}

[System.Serializable]
public class BulletTag : IActionGroup, NamedObject
{
	[SerializeField]
	private string btName = "Bullet Tag";
	[SerializeField]
	public AttackPattern.Property speed;
	[SerializeField]
	public GameObject prefab;
	[SerializeField]
	public BulletAction[] actions;
	
	public string Name
	{
		get
		{
			return btName;
		}
		
		set
		{
			btName = value;
		}
	}

	public BulletTag()
	{
		actions = new BulletAction[0];
	}
	
	public IEnumerator Run (params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			yield return actions[0].parent.StartCoroutine (ActionHandler.ExecuteActions(actions, param));
		}
	}
	
	public void Initialize(MonoBehaviour parent)
	{
		foreach(BulletAction action in actions)
		{
			action.Initialize(parent);
		}
	}
	
	#if UNITY_EDITOR
	public void ActionGUI (params object[] param)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new BulletAction[0];
		}
		
		EditorGUILayout.LabelField("Bullet Tag: " + Name);
		prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
		speed = AttackPattern.Property.EditorGUI ("Speed", speed, false);
		EditorUtils.ExpandCollapseButtons<BulletAction, BulletAction.Type>("Actions", actions);
		actions = EditorUtils.ActionGUI<BulletAction, BulletAction.Type>(actions, true, param[0] as Enemy, param);
	}

	public void DrawGizmos(Color gizmoColor)
	{

	}
	#endif
}
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Stage : CachedObject, IActionGroup
{
	public int nextStageSceneNumber;
	public StageAction[] actions;

	[NonSerialized]
	public Vector3 sequenceLocation = Vector3.zero;

	public IEnumerator Run (params object[] param)
	{		
		if(actions != null && actions.Length > 0)
		{
			running = true;
			yield return StartCoroutine(ActionHandler.ExecuteActions(actions, param));
			running = false;
		}
		StageManager.EndStage ();
	}
	
	private bool running;
	
	public Color gizmoColor = Color.cyan;
	
	public int Size
	{
		get { return actions.Length; }
	}
	
	public void Initialize(MonoBehaviour parent)
	{
		foreach(StageAction action in actions)
		{
			action.Initialize(this);
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
		Initialize (this);
		StartCoroutine(Run(this));
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
			actions = new StageAction[1];
			actions [0] = new StageAction();
		}
		
		EditorUtils.ExpandCollapseButtons<StageAction, StageAction.Type>("Stage", actions);
		
		actions = EditorUtils.ActionGUI<StageAction, StageAction.Type>(actions, false, this);
	}
	
	public void DrawGizmos(Color gizmoColor)
	{
		ActionHandler.DrawActionGizmos<StageAction, StageAction.Type> (actions, gizmoColor);
	}
	#endif
}

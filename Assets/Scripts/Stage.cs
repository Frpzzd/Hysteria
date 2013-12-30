using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Stage : AbstractActionBehavior<StageAction, StageAction.Type>, IActionGroup
{
	public override string ActionGUITitle { get { return "Stage"; } }
	public override object[] ActionParameters { get { return null; } }
	public int nextStageSceneNumber;

	[NonSerialized]
	public Vector3 sequenceLocation = Vector3.zero;

	public override IEnumerator Run (params object[] param)
	{
		yield return StartCoroutine (base.Run (param));
		StageManager.EndStage ();
	}
}

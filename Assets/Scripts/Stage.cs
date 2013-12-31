using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Stage : CachedObject, IActionGroup
{
	public int nextStageSceneNumber;
	public Action[] actions;

	[NonSerialized]
	public Vector3 sequenceLocation = Vector3.zero;

	public IEnumerator Run (params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			running = true;
			for(int i = 0; i < actions.Length; i++)
			{
				yield return StartCoroutine(actions[i].Execute(this));
			}
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
		foreach(Action action in actions)
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
			actions = new Action[1];
			actions [0] = new Action();
		}
		
		EditorUtils.ExpandCollapseButtons<Action, Action.Type>("Stage", actions);
		
		actions = EditorUtils.ActionGUI<Action, Action.Type>(actions, false, this);
	}
	
	public void DrawGizmos(Color gizmoColor)
	{
		ActionHandler.DrawActionGizmos<Action, Action.Type> (actions, gizmoColor);
	}
	#endif

	
	[Serializable]
	public class Action : NestedAction<Stage.Action, Stage.Action.Type>
	{
		public enum Type { Wait, Repeat, SpawnEnemy, PlayMusic }
		
		[SerializeField]
		public GameObject prefab;
		[SerializeField]
		public Vector2 location;
		[SerializeField]
		public bool useSequence;
		[SerializeField]
		public AudioClip music;
		
		#if UNITY_EDITOR
		public override void ActionGUI(params object[] param)
		{		
			type = (Type)EditorGUILayout.EnumPopup("Type", type);
			parent = param [0] as MonoBehaviour;
			switch(type)
			{
				case Type.Wait:
					wait = AttackPattern.Property.EditorGUI("Wait", wait, false);
					break;
				case Type.Repeat:
					SharedAction.Repeat.ActionGUI<Stage.Action, Stage.Action.Type>(this, parent);
					break;
				case Type.SpawnEnemy:
					prefab = (GameObject)EditorGUILayout.ObjectField ("Enemy", prefab, typeof(GameObject), false);
					location = EditorGUILayout.Vector2Field ("Spawn Location", location);
					useSequence = EditorGUILayout.Toggle("Use Sequence", useSequence);
					break;
				case Type.PlayMusic:
					music = (AudioClip)EditorGUILayout.ObjectField("Music Clip", music, typeof(AudioClip), false);
					break;
			}
		}
		
		public override void DrawGizmosImpl (Stage.Action previous)
		{
			if(type == Type.Repeat)
			{
				SharedAction.Repeat.DrawGizmos<Stage.Action, Stage.Action.Type>(this);
			}
			if(type == Type.SpawnEnemy)
			{
				Gizmos.DrawWireSphere(GetSpawnLocation(), 1);
			}
		}
		#endif
		
		private Vector3 GetSpawnLocation()
		{
			Vector3 targetSpawnLocation = location;
			Stage master = parent as Stage;
			if(useSequence)
			{
				master.sequenceLocation += targetSpawnLocation;
			}
			else
			{
				master.sequenceLocation = targetSpawnLocation;
			}
			return master.sequenceLocation;
		}
		
		public override IEnumerator Execute (params object[] param)
		{
			switch(type)
			{
				case Type.SpawnEnemy:
					Vector3 spawnLocation = GetSpawnLocation();
					Enemy instance = ((GameObject)UnityEngine.Object.Instantiate (prefab)).GetComponent<Enemy> ();
					instance.Transform.position = new Vector3 (spawnLocation.x, spawnLocation.y);
					break;
				case Type.PlayMusic:
					SoundManager.PlayMusic(music);
					break;
				case Type.Repeat:
					int repeatC = Mathf.FloorToInt(repeat.Value);
					for(int j = 0; j < repeatC; j++)
					{
						for(int i = 0; i < nestedActions.Length; i++)
						{
							yield return parent.StartCoroutine(nestedActions[i].Execute());
						}
					}
					break;
				case Type.Wait:
					yield return new WaitForSeconds(wait.Value);
					break;
			}
		}
	}
}

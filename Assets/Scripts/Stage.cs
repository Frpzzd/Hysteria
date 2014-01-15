using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[RequireComponent(typeof(Enemy))]
public class Stage : CachedObject, IActionGroup
{
	public int nextStageSceneNumber;
	public Action[] actions;
	[HideInInspector]
	public Enemy boss;
	public AudioClip stageTheme;
	public int clearBonus;

	[NonSerialized]
	public Vector3 sequenceLocation = Vector3.zero;

	public IEnumerator Run (params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			running = true;
			SoundManager.PlayMusic(stageTheme);
			IEnumerator pause, actionEnumerator;
			foreach(Action action in actions)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				actionEnumerator = action.Execute(this);
				while(actionEnumerator.MoveNext())
				{
					yield return actionEnumerator.Current;
				}
			}
			yield return StartCoroutine(BossGUI.Instance.BossBattle(boss, this));
			running = false;
		}
		yield return StartCoroutine(StageManager.EndStage (clearBonus));
		Destroy (GameObject); //Clean up and destroy all stage related GameObjects, which should be child GameObjects to this one
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
		boss = GetComponent<Enemy> ();
	}

	public void OnEnable()
	{
		if(!running)
		{
			StartActions();
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

		stageTheme = (AudioClip)EditorGUILayout.ObjectField ("Theme", stageTheme, typeof(AudioClip), false);
		clearBonus = EditorGUILayout.IntField ("Clear Bonus", clearBonus);

		EditorUtils.ExpandCollapseButtons<Action, Action.Type>("Actions:", actions);
		
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
		public enum Type { Wait, Repeat, SpawnEnemy }
		
		[SerializeField]
		public GameObject prefab;
		[SerializeField]
		public Vector2 location;
		[SerializeField]
		public bool useSequence;
		
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
				Vector3 spawnLocation = GetSpawnLocation();
				if(prefab != null)
				{
					prefab.GetComponent<Enemy>().DrawGizmos(spawnLocation);
				}
				Gizmos.DrawWireSphere(spawnLocation, 1);
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
			IEnumerator pause, actionEnumerator;
			switch(type)
			{
				case Type.SpawnEnemy:
					Vector3 spawnLocation = GetSpawnLocation();
					Enemy instance = ((GameObject)UnityEngine.Object.Instantiate (prefab)).GetComponent<Enemy> ();
					instance.Transform.position = new Vector3 (spawnLocation.x, spawnLocation.y);
					instance.Spawn();
					break;
				case Type.Repeat:
					int repeatC = Mathf.FloorToInt(repeat.Value);
					for(int j = 0; j < repeatC; j++)
					{
						foreach(Action action in nestedActions)
						{
							pause = Global.WaitForUnpause();
							while(pause.MoveNext())
							{
								yield return pause.Current;
							}
							actionEnumerator = action.Execute();
							while(actionEnumerator.MoveNext())
							{
								yield return actionEnumerator.Current;
							}
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

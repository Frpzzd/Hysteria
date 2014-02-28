using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using DanmakuEngine.Core;

namespace DanmakuEngine.Actions
{
	[Serializable]
	[AddComponentMenu("Danmaku Engine/Action/Stage")]
	public class ActionStage : AbstractStage
	{
		public int nextStageSceneNumber;
		public Action[] actions;
		
		[NonSerialized]
		public Vector3 sequenceLocation = Vector3.zero;
		
		public IEnumerator Run (params object[] param)
		{
			if(actions != null && actions.Length > 0)
			{
				long start = (long)Time.fixedTime;
				running = true;
				SoundManager.PlayMusic(theme);
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
				Debug.Log(Time.fixedTime - start);
				running = false;
			}
			yield return StartCoroutine(StageManager.EndStage (bonus));
			Debug.Log ("Enemy Dead");
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

		[Serializable]
		public class Action : NestedAction<ActionStage.Action, ActionStage.Action.Type>
		{
			public enum Type { Wait, Repeat, SpawnEnemy }
			[SerializeField]
			public GameObject prefab;
			[SerializeField]
			public Vector2 location;
			[SerializeField]
			public bool useSequence;
			[SerializeField]
			public bool mirrorMovementX;
			[SerializeField]
			public bool mirrorMovementY;
			
			protected override void DrawHandlesImpl (Action previous)
			{
				if(type == Type.Repeat)
				{
					RepeatHandles(this, Handles.color);
				}
				if(type == Type.SpawnEnemy)
				{
					Vector3 spawnLocation = GetSpawnLocation();
					if(prefab != null)
					{
						Enemy e = prefab.GetComponent<Enemy>();
						if(e != null)
						{
							e.DrawHandles(spawnLocation, mirrorMovementX, mirrorMovementY, Color.yellow);
						}
					}
					Handles.DrawWireDisc(spawnLocation, Vector3.forward, 1);
				}
			}
			
			private Vector3 GetSpawnLocation()
			{
				Vector3 targetSpawnLocation = location;
				ActionStage master = parent as ActionStage;
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
					AbstractEnemy instance = ((GameObject)UnityEngine.Object.Instantiate (prefab)).GetComponent<AbstractEnemy> ();
					instance.Transform.position = new Vector3 (spawnLocation.x, spawnLocation.y);
					if(instance is Enemy)
					{
						Enemy e = instance as Enemy;
						e.mirrorMovementX = mirrorMovementX;
						e.mirrorMovementY = mirrorMovementY;
						instance.Spawn();
					}
					else if(instance is Boss)
					{
						yield return parent.StartCoroutine(BossGUI.Instance.BossBattle(instance as Boss, parent as AbstractStage));
					}
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
}

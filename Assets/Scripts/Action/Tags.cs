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
	[SerializeField]
	public bool runAtStart = true;
	[SerializeField]
	public bool loopUntilEnd = true;
	[SerializeField]
	public AttackPattern.Property wait;
	[SerializeField]
	public bool hasWait;

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
		Enemy enemy = param [0] as Enemy;
		AttackPattern pattern = param [1] as AttackPattern;
		IEnumerator pause, actionEnumerator;
		if(actions != null && actions.Length > 0)
		{
			if(loopUntilEnd)
			{
				while(pattern.currentHealth > 0)
				{
					foreach(Action action in actions)
					{
						pause = Global.WaitForUnpause();
						while(pause.MoveNext())
						{
							yield return pause.Current;
						}
						actionEnumerator = action.Execute(enemy, pattern, this);
						while(actionEnumerator.MoveNext())
						{
							yield return actionEnumerator.Current;
						}
					}
					float waitC = wait.Value;
					if(waitC <= Time.fixedDeltaTime)
					{
						yield return new WaitForFixedUpdate();
					}
					else
					{
						yield return new WaitForSeconds(waitC);
					}
				}
			}
			else
			{
				foreach(Action action in actions)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
					actionEnumerator = action.Execute(enemy, pattern, this);
					while(actionEnumerator.MoveNext())
					{
						yield return actionEnumerator.Current;
					}
				}
			}
		}
	}

	public void Initialize(MonoBehaviour parent)
	{
		foreach(FireAction action in actions)
		{
			action.Initialize(parent);
		}
	}

	public bool CheckForWait(AttackPattern pattern)
	{
		hasWait = false;
		for(int i = 0; i < actions.Length; i++)
		{
			hasWait |= actions[i].CheckForWait(pattern);
		}
		return hasWait;
	}
	
	#if UNITY_EDITOR
	public void ActionGUI (params object[] param)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new FireAction[1];
			actions [0] = new FireAction();
		}

		EditorGUILayout.LabelField ("Fire Tag: " + Name);

		EditorGUILayout.BeginHorizontal ();
		runAtStart = EditorGUILayout.Toggle ("Run At Start", runAtStart);
		loopUntilEnd = EditorGUILayout.Toggle ("Loop until End", loopUntilEnd);
		EditorGUILayout.EndHorizontal ();
		if(loopUntilEnd)
		{
			wait = AttackPattern.Property.EditorGUI("Loop Wait", wait, false);
		}

		
		EditorUtils.ExpandCollapseButtons<FireAction, FireAction.Type>("Actions:", actions);
		
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
	public Bullet.Action[] actions;
	[SerializeField]
	public bool overwriteColor = false;
	[SerializeField]
	public Color newColor = Color.white;
	
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
		actions = new Bullet.Action[0];
	}
	
	public IEnumerator Run (params object[] param)
	{
		if(actions != null && actions.Length > 0)
		{
			IEnumerator pause, actionEnumerator;
			foreach(Action action in actions)
			{
				pause = Global.WaitForUnpause();
				while(pause.MoveNext())
				{
					yield return pause.Current;
				}
				actionEnumerator = action.Execute(this, param[0] as AttackPattern);
				while(actionEnumerator.MoveNext())
				{
					yield return actionEnumerator.Current;
				}
			}
		}
	}
	
	public void Initialize(MonoBehaviour parent)
	{
		foreach(Bullet.Action action in actions)
		{
			action.Initialize(parent);
		}
	}
	
	#if UNITY_EDITOR
	public void ActionGUI (params object[] param)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new Bullet.Action[0];
		}
		
		EditorGUILayout.LabelField("Bullet Tag: " + Name);
		prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
		speed = AttackPattern.Property.EditorGUI ("Speed", speed, false);
		overwriteColor = EditorGUILayout.Toggle ("Overwrite Prefab Color:", overwriteColor);
		if(overwriteColor)
		{
			newColor = EditorGUILayout.ColorField ("New Color", newColor);
		}
		EditorUtils.ExpandCollapseButtons<Bullet.Action, Bullet.Action.Type>("Actions", actions);
		actions = EditorUtils.ActionGUI<Bullet.Action, Bullet.Action.Type>(actions, true, param[0] as Enemy, param);
	}

	public void DrawGizmos(Color gizmoColor)
	{

	}
	#endif
}
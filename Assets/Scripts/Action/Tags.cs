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
		if(actions != null && actions.Length > 0)
		{
			if(loopUntilEnd)
			{
				while(pattern.currentHealth > 0)
				{
					for(int i = 0; i < actions.Length; i++)
					{
						yield return actions[i].parent.StartCoroutine(Global.WaitForUnpause());
						if(pattern.currentHealth < 0)
						{
							return false;
						}
						yield return actions[i].parent.StartCoroutine(actions[i].Execute(enemy, pattern, this));
					}
				}
			}
			else
			{
				for(int i = 0; i < actions.Length; i++)
				{
					yield return actions[i].parent.StartCoroutine(Global.WaitForUnpause());
					if(pattern.currentHealth < 0)
					{
						return false;
					}
					yield return actions[i].parent.StartCoroutine(actions[i].Execute(this, pattern));
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
			for(int i = 0; i < actions.Length; i++)
			{
				yield return actions[i].parent.StartCoroutine(actions[i].Execute(this, param[0] as AttackPattern));
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
		EditorUtils.ExpandCollapseButtons<Bullet.Action, Bullet.Action.Type>("Actions", actions);
		actions = EditorUtils.ActionGUI<Bullet.Action, Bullet.Action.Type>(actions, true, param[0] as Enemy, param);
	}

	public void DrawGizmos(Color gizmoColor)
	{

	}
	#endif
}
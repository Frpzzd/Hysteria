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

/// <summary>
/// Fire Tag
/// A group of actions that describe a firing pattern used in a AttackPattern
/// </summary>
[System.Serializable]
public class FireTag : IActionGroup, NamedObject
{
	[SerializeField]
	private string ftName = "Fire Tag";

	/// <summary>
	/// The actions executed by this FireTag
	/// </summary>
	[SerializeField]
	public FireAction[] actions;

	/// <summary>
	/// Whether this FireTag is start when the AttackPattern starts
	/// </summary>
	[SerializeField]
	public bool runAtStart = true;

	/// <summary>
	/// If this value is true, this FireTag will loop until the AttackPattern this FireTag belongs
	/// to ends.
	/// </summary>
	[SerializeField]
	public bool loopUntilEnd = true;

	/// <summary>
	/// If this tag loops until the end of the AttackPattern, this AttackPattern Property determines
	/// how long it waits between each loop
	/// </summary>
	[SerializeField]
	public AttackPattern.Property wait;

	/// <summary>
	/// Gets or sets the name of the FireTag
	/// This is mainly used by the Editor to differentiate between different FireTags
	/// and has no actual impact on the execution of game.
	/// </summary>
	/// <value>The name.</value>
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

	/// <summary>
	/// Initializes a new instance of the <see cref="FireTag"/> class.
	/// </summary>
	public FireTag()
	{
		actions = new FireAction[1];
		actions [0] = new FireAction ();
	}

	/// <summary>
	/// Executes the FireTag as specified 
	/// </summary>
	/// <param name="param">Parameter.</param>
	public IEnumerator Run (params object[] param)
	{
		Enemy enemy = param [0] as Enemy;
		AttackPattern pattern = param [1] as AttackPattern;
		float floatParam = (param.Length > 2) ? (float)param [2] : 0.0f;
		RotationWrapper prevRotation = new RotationWrapper ();
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
						actionEnumerator = action.Execute(enemy, pattern, floatParam, prevRotation);
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
						float currentTime = 0f;
						while(currentTime < waitC)
						{
							pause = Global.WaitForUnpause();
							while(pause.MoveNext())
							{
								yield return pause.Current;
							}
							yield return new WaitForFixedUpdate();
							currentTime += Time.fixedDeltaTime;
						}
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
					actionEnumerator = action.Execute(enemy, pattern, floatParam, prevRotation);
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
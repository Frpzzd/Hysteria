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

public interface ITag
{
	void ActionGUI(AttackPattern attackPattern);
}

[Serializable]
public abstract class Tag<T> : AbstractActionGroup<T>, ITag where T : Action
{
	public abstract void ActionGUI(AttackPattern attackPattern);
}

[Serializable]
public class FireTag : Tag<IFireAction>, NamedObject
{
	private string ftName = "Fire Tag";
	public float param = 0.0f;
	public RotationWrapper previousRotation;
	
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

	public override void Run(params object[] param)
	{
		for(int i = 0; i < actions.Length; i++)
		{
			ActionExecutor.ExecuteAction(actions[i], this);
		}
	}

	public override void ActionGUI (AttackPattern attackPattern)
	{
		#if UNITY_EDITOR
		if (actions == null || actions.Length == 0)
		{
			actions = new IFireAction[1];
			actions [0] = new SharedAction.Wait();
		}
		
		EditorUtils.ExpandCollapseButtons("Fire Tag: " + Name, actions);
		
		actions = EditorUtils.ActionGUI<IFireAction, SharedAction.Wait> (actions, false, attackPattern);
		#endif
	}
}

public class BulletTag : Tag<IBulletAction>, NamedObject
{
	private string btName = "Bullet Tag";
	public AttackPattern.Property speed;
	public GameObject prefab;
	
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

	public override void Run(params object[] param)
	{

	}

	public override void ActionGUI (AttackPattern attackPattern)
	{
		#if UNITY_EDITOR
		if (actions == null || actions.Length == 0)
		{
			actions = new IBulletAction[0];
		}

		EditorGUILayout.LabelField("Bullet Tag: " + Name);
		speed.EditorGUI ("Speed", false);
		EditorUtils.ExpandCollapseButtons("Actions", actions);
		actions = EditorUtils.ActionGUI<IBulletAction, SharedAction.Wait>(actions, true, attackPattern);
		#endif
	}
}
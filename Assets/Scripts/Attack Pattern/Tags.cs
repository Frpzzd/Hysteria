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

[Serializable]
public abstract class Tag
{
	public abstract void ActionGUI(AttackPattern attackPattern);
}

[Serializable]
public class FireTag : Tag, NamedObject
{
	private string ftName = "Fire Tag";
	public float param = 0.0f;
	public RotationWrapper previousRotation;
	public IFireAction[] actions;
	
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
	public override void ActionGUI (AttackPattern attackPattern)
	{
		#if UNITY_EDITOR
		if (actions == null || actions.Length == 0)
		{
			actions = new IFireAction[1];
			actions [0] = new SharedAction.Wait();
		}
		
		EditorUtils.ExpandCollapseButtons("Fire Tag: " + Name, actions);
		
		actions = EditorUtils.FireActionGUI(actions, attackPattern); 
		#endif
	}
}

public class BulletTag : Tag, NamedObject
{
	private string btName = "Bullet Tag";
	public AttackPattern.Property speed;
	public GameObject prefab;
	
	public IBulletAction[] actions;
	
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

	public override void ActionGUI (AttackPattern attackPattern)
	{
		#if UNITY_EDITOR
		if (actions == null || actions.Length == 0)
		{
			actions = new BulletAction[0];
		}

		EditorGUILayout.LabelField("Bullet Tag: " + Name);
		speed.EditorGUI ("Speed", false);
		EditorUtils.ExpandCollapseButtons("Actions", actions);
		actions = EditorUtils.BulletActionGUI (actions, attackPattern);
		#endif
	}
}
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
public abstract class Tag<T, P> : AbstractActionGroup<T, P> where T : NestedAction<T, P> where P : struct, IConvertible
{
	#if UNITY_EDITOR
	public override void ActionGUI(params object[] param)
	{
		ActionGUI (param [0] as AttackPattern);
	}

	public abstract void ActionGUI(AttackPattern attackPattern);
	#endif
}

[Serializable]
public class FireTag : Tag<FireAction, FireAction.Type>, NamedObject
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

	public override object[] AlternateParameters (params object[] param)
	{
		return new object[]{ this };
	}
	
	#if UNITY_EDITOR
	public override void ActionGUI (AttackPattern attackPattern)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new FireAction[1];
			actions [0] = new FireAction();
		}
		
		EditorUtils.ExpandCollapseButtons<FireAction, FireAction.Type>("Fire Tag: " + Name, actions);
		
		actions = EditorUtils.ActionGUI<FireAction, FireAction.Type> (actions, false, attackPattern);
	}
	#endif
}

public class BulletTag : Tag<BulletAction, BulletAction.Type>, NamedObject
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
	
	#if UNITY_EDITOR
	public override void ActionGUI (AttackPattern attackPattern)
	{
		if (actions == null || actions.Length == 0)
		{
			actions = new BulletAction[0];
		}

		EditorGUILayout.LabelField("Bullet Tag: " + Name);
		prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
		speed = AttackPattern.Property.EditorGUI ("Speed", speed, false);
		EditorUtils.ExpandCollapseButtons<BulletAction, BulletAction.Type>("Actions", actions);
		actions = EditorUtils.ActionGUI<BulletAction, BulletAction.Type>(actions, true, attackPattern);
	}
	#endif
}
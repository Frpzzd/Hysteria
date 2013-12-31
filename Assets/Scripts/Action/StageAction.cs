using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class StageAction : NestedAction<StageAction, StageAction.Type>
{
	public override ActionType ActionType { get { return ActionType.Normal; } }
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
		switch(type)
		{
			case Type.Wait:
				wait = AttackPattern.Property.EditorGUI("Wait", wait, false);
				break;
			case Type.Repeat:
				SharedAction.Repeat.ActionGUI<StageAction, StageAction.Type>(this, parent);
				break;
			case Type.SpawnEnemy:
				prefab = (GameObject)EditorGUILayout.ObjectField ("Enemy", prefab, typeof(GameObject), false);
				location = EditorGUILayout.Vector2Field ("Spawn Location", location);
				useSequence = EditorGUILayout.Toggle("Use Sequence", useSequence);
				break;
			case Type.PlayMusic:
				music = (AudioClip)EditorGUILayout.ObjectField("Music CLip", music, typeof(AudioClip), false);
				break;
		}
	}

	public override void DrawGizmosImpl (StageAction previous)
	{
		if(type == Type.Repeat)
		{
			SharedAction.Repeat.DrawGizmos<StageAction, StageAction.Type>(this);
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
				yield return parent.StartCoroutine(SharedAction.Repeat.Execute<StageAction, StageAction.Type>(nestedActions, repeat));
				break;
			case Type.Wait:
				yield return new WaitForSeconds(wait.Value);
				break;
		}
	}
}
using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class TestAttackPattern : MonoBehaviour 
{
	public AttackPattern patternToTest;
	public Enemy enemyToAttachTo;
	public Rank testRank;

	void Start()
	{
		if(EditorApplication.isPlayingOrWillChangePlaymode)
		{
			Debug.Log("Start");
			Global.Rank = testRank;
			patternToTest.Initialize(enemyToAttachTo);
			enemyToAttachTo.StartCoroutine(patternToTest.Run());
		}
		else
		{
			Debug.Log("Destroy");
			DestroyImmediate(gameObject);
		}
	}
}

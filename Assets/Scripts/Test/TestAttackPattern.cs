using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TestAttackPattern : MonoBehaviour 
{
	public int patternToTest;
	public Enemy enemyToAttachTo;
	public Rank testRank;

	void Start()
	{
		#if UNITY_EDITOR
		if(EditorApplication.isPlayingOrWillChangePlaymode)
		{
			Global.Rank = testRank;
			enemyToAttachTo.TestAttackPattern(patternToTest);
		}
		else
		{
			DestroyImmediate(gameObject);
		}
		#else
		Destroy(gameObject);
		#endif
	}

	void OnDestroy()
	{
		#if UNITY_EDITOR
		Debug.Log("Destroy");
		#endif
	}
}

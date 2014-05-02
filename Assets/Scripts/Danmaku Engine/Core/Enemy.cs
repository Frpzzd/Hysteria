using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace DanmakuEngine.Core
{
	[System.Serializable]
	public class Enemy : AbstractEnemy
	{
		[SerializeField]
		public string enemyName = "Enemy";
		[System.NonSerialized]
		public bool mirrorMovementX = false;
		[System.NonSerialized]
		public bool mirrorMovementY = false;

		public override AbstractAttackPattern currentAttackPattern 
		{ 
			get { return attackPattern; } 
		}
		
		[SerializeField]
		public AbstractAttackPattern attackPattern;
		[SerializeField]
		public AbstractAttackPattern deathPattern;
		
		public override string Name
		{
			get 
			{
				return enemyName;
			}
			
			set
			{
				enemyName = value;
			}
		}
		
		public int apSelect;
		
		protected override IEnumerator Run()
		{
			if(attackPattern != null)
			{
				attackPattern.Initialize(this);
				yield return StartCoroutine(attackPattern.Run());
				attackPattern.drops.Drop (Transform.position);
			}
			Die ();
		}
	
//		public void TestAttackPattern(int ap)
//		{
//			apSelect = ap;
//			StartCoroutine (TestRunAttackPattern ());
//		}
//		
//		public IEnumerator TestRunAttackPattern()
//		{
//			float start = Time.time;
//			attackPattern.Initialize(this);
//			yield return StartCoroutine(attackPattern.Run());
//			Drop (attackPattern.drops);
//			Die ();
//			Debug.Log (Time.time - start + " total seconds.");
//		}

		public void DrawHandles(Vector3 spawnPosition, bool mirrorMoveX, bool mirrorMoveY, Color handleColor)
		{
			#if UNITY_EDITOR
			Color oldColor = Handles.color;
			Handles.color = Color.yellow;
			Vector3 endLocation = spawnPosition;
			if(attackPattern != null)
			{
				attackPattern.DrawHandles (endLocation, mirrorMoveX, mirrorMoveY, Color.yellow);
			}
			Handles.color = oldColor;
			#endif
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DanmakuEngine.Core;

namespace DanmakuEngine.Core
{
	[System.Serializable]
	public class Boss : AbstractEnemy, TitledObject
	{
		[SerializeField]
		public string bossName = "Boss";
		[SerializeField]
		public string bossTitle = "Title";
		[HideInInspector]
		public int apSelect;
		[SerializeField]
		public AbstractAttackPattern[] attackPatterns;
		public AudioClip theme;
		public Vector2 startVector;
		[System.NonSerialized]
		public bool mirrorMovement = false;
		
		public override string Name
		{
			get 
			{
				return bossName;
			}
			
			set
			{
				bossName = value;
			}
		}
		
		public string Title
		{
			get 
			{
				if(bossTitle == null)
				{
					bossTitle = "";
				}
				return bossTitle;
			}
			
			set
			{
				bossTitle = value;
			}
		}

		public override AbstractAttackPattern currentAttackPattern
		{
			get 
			{ 
				return (apSelect >= 0 && apSelect < attackPatterns.Length) ? attackPatterns [apSelect] : null;
			}
		}
		
		public int RemainingAttackPatterns
		{
			get { return attackPatterns.Length - apSelect; }
		}
		
		protected override IEnumerator Run()
		{
			for(apSelect = 0; apSelect < attackPatterns.Length; apSelect++)
			{
				float t = 0f;
				Vector3 start = Transform.position;
				while(t < 1f)
				{
					Transform.position = Vector3.Lerp(start, startVector, t);
					t += Time.fixedDeltaTime;
					yield return new WaitForFixedUpdate();
				}
				currentAttackPattern.Initialize(this);
				yield return StartCoroutine(currentAttackPattern.Run());
				if(currentAttackPattern.success)
				{
					currentAttackPattern.drops.Drop(Transform.position);
				}
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
//			currentAttackPattern.Initialize(this);
//			yield return StartCoroutine(currentAttackPattern.Run());
//			Drop (currentAttackPattern.drops);
//			Die ();
//			Debug.Log (Time.time - start + " total seconds.");
//		}
	}

}
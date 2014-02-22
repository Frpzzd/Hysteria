using UnityEngine;
using System.Collections.Generic;
using DanmakuEngine.Core;

namespace DanmakuEngine.Core
{
	[System.Serializable]
	[RequireComponent(typeof(SpriteRenderer))]
	public class PlayerShot : PooledGameObject<PlayerShot, PlayerShotPool>
	{
		[System.NonSerialized]
		public bool mainShot;
		[System.NonSerialized]
		public float speed;
		[HideInInspector]
		public AbstractEnemy target;
		[System.NonSerialized]
		public List<AbstractEnemy> enemiesHit;
		[System.NonSerialized]
		public SpriteRenderer rend;

		[SerializeField]
		public LayerMask enemyMask;
		
		[SerializeField]
		public float mainSpeed;
		[SerializeField]
		public float optionSpeed;
		[SerializeField]
		public Color mainColor;
		[SerializeField]
		public Color optionColor;
		
		public int DamageValue
		{
			get 
			{
				if(mainShot)
				{
					return Player.MainShotDamage;
				}
				else
				{
					return Player.OptionShotDamage;
				}
			}
		}
		
		public override void Awake()
		{
			base.Awake ();
			enemiesHit = new List<AbstractEnemy> ();
			rend = renderer as SpriteRenderer;
		}
		
		public override void Activate (object[] param)
		{
			mainShot = (bool)param[0];
			enemiesHit.Clear ();
			if(!mainShot)
			{
				if(Player.Judging)
				{
//					float closestDistance = float.MaxValue;
//					for(int i = 0; i < Enemy.enemiesInPlay.Count; i++)
//					{
//						float currentDistance = (Enemy.enemiesInPlay[i].Transform.position - Player.PlayerTransform.position).sqrMagnitude;
//						if(currentDistance < closestDistance)
//						{
//							target = Enemy.enemiesInPlay[i];
//						}
//					}
				}
				rend.color = optionColor;
				speed = optionSpeed;
				name = "Option Shot";
			}
			else
			{
				target = null;
				rend.color = mainColor;
				speed = mainSpeed;
				name = "Main Shot";
			}
		}
		
		// Update is called once per frame
		void FixedUpdate () 
		{
			//Raycast check for enemies
			float distance = speed * Time.fixedDeltaTime;
			Debug.DrawRay (Transform.position, Transform.up * speed * Time.deltaTime);
			RaycastHit2D raycastHit = Physics2D.Raycast(Transform.position.XY(), Transform.up.XY(), distance, enemyMask);
			if(raycastHit)
			{
				Enemy enemy = raycastHit.collider.gameObject.GetComponent<Enemy>();
				if(enemy != null)
				{
					if(enemiesHit.Contains(enemy))
					{
						Transform.position  += Transform.up * distance;
					}
					else
					{
						enemy.Damage(DamageValue);
						Transform.position = raycastHit.point.ToVector3();
						//TODO: Play Enemy hit effect here;
						if(!Player.Percieving || mainShot)
						{
							PlayerShotPool.Return(this);
						}
						else
						{
							enemiesHit.Add(enemy);
						}
					}
				}
				else
				{
					Debug.Log("Hit Something that isn't an enemy" + raycastHit.collider.gameObject);
				}
			}
			else
			{
				Transform.position  += Transform.up * distance;
				
				//TODO: Complete Homing shot code
				if(target != null && !target.Dead)
				{
					
				}
			}
		}
	}
}
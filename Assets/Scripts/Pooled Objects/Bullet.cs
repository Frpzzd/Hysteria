using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : GameObjectManager.PooledGameObject<Bullet, BulletTag>
{
	[HideInInspector]
	public AttackPattern master;
	private BulletTag bulletTag;
	[HideInInspector]
	public GameObject prefab;
	[HideInInspector]
	public RotationWrapper prevRotation = new RotationWrapper();
	
	[HideInInspector]
	public float speed = 5.0f;
	[HideInInspector]
	public float verticalSpeed = 0.0f;
	[HideInInspector]
	public bool useVertical = false;
	[HideInInspector]
	public bool grazed;
	[HideInInspector]
	public float param = 0.0f;
	[HideInInspector]
	public Collider2D col;
	private SpriteRenderer rend;

	public override void Awake ()
	{
		base.Awake ();
		col = collider2D;
		rend = (SpriteRenderer)renderer;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		Vector3 velocity = Transform.up * speed * Time.fixedDeltaTime;
		if(useVertical)
		{
			velocity += (Vector3.up * verticalSpeed * Time.fixedDeltaTime);
		}
		Transform.position += velocity;
	}

	public override void Activate (BulletTag param)
	{
		grazed = false;
		if(prefab != param.prefab)
		{
			prefab = param.prefab;
			SpriteRenderer sp = prefab.renderer as SpriteRenderer;
			rend.color = sp.color;
			rend.sprite = sp.sprite;
		}
		bulletTag = param;
	}

	public override void LateActivate ()
	{
		bulletTag.Run (this);
	}

	public void Deactivate()
	{
		if(GameObject.activeSelf)
		{
			GameObjectManager.Bullets.Return(this);
		}
	}

//	public IEnumerator RunActions(IBulletAction[] actions, int repeatC)
//	{
//		for(int j = 0; j < repeatC; j++)
//		{
//			for(int i = 0; i < actions.Length; i++)
//			{
//				ActionExecutor.ExecuteAction(actions[i], this);
//				switch(actions[i].type)
//				{
//					case(BulletAction.Type.Wait):
//						yield return new WaitForSeconds(actions[i].wait.Value * deltat);
//						break;
//					case(BulletAction.Type.ChangeDirection):
//						if(actions[i].waitForChange)
//						{
//							yield return ChangeDirection(actions[i]);
//						}
//						else
//						{
//							ChangeDirection(actions[i]);
//						}
//						break;
//					case(BulletAction.Type.ChangeSpeed):
//						if(actions[i].waitForChange)
//						{
//							yield return ChangeSpeed(actions[i], false);
//						}
//						else
//						{
//							ChangeSpeed(actions[i], false);
//						}
//						break;
//					case(BulletAction.Type.Repeat):
//						yield return RunActions(actions[i].nestedActions, Mathf.FloorToInt(actions[i].repeat.Value));
//						break;
//					case(BulletAction.Type.Fire):
//						master.Fire(actions[i].GetSourcePosition(Transform.position), Transform.rotation, actions[i], param, prevRotation);
//						break;
//					case(BulletAction.Type.VerticalChangeSpeed):
//						if(actions[i].waitForChange)
//						{
//							yield return ChangeSpeed(actions[i], true);
//						}
//						else
//						{
//							ChangeSpeed(actions[i], true);
//						}
//						break;
//					case(BulletAction.Type.Deactivate):
//						Deactivate();
//						break;
//				}
//			}
//		}
//	}

	public void Cancel()
	{
		GameObjectManager.Pickups.Spawn (Transform.position, PickupType.PointValue);
		Deactivate();
	}
}
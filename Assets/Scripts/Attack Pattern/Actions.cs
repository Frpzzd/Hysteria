using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ActionType { Normal, Yield, Coroutine }

public interface Action
{
	ActionType Type { get; }
	AttackPattern Master { get; }

#if UNITY_EDITOR
	bool Foldout { get; set; }

	void ActionGUI(AttackPattern master);
	void DrawHandles();
#endif
}

public interface INestedAction
{
#if UNITY_EDITOR
	void Expand (bool recursive);
	void Collapse(bool recursive);
	void SetAll (bool value, bool recursive);
#endif
}

public abstract class AbstractAction : Action
{
	#if UNITY_EDITOR
	private bool foldout = true;
	public bool Foldout
	{
		get { return foldout; }
		set { foldout = value; }
	}

	public abstract void ActionGUI(AttackPattern master);
	public abstract void DrawHandles();
	#endif

	protected AttackPattern master;
	public abstract ActionType Type{ get; }

	public AttackPattern Master
	{
		get { return master; }
	}

	public override string ToString ()
	{
		#if UNITY_EDITOR
		return EditorUtils.TypeStruct.ProcessName(GetType());
		#else
		return base.ToString();
		#endif
	}
}

public abstract class SharedAction : AbstractAction, IFireAction, IBulletAction
{
	public bool WaitForChange;
	public AttackPattern.Property wait;
	public abstract void Execute(FireTag tag);
	public abstract YieldInstruction YieldExecute(FireTag tag);
	public abstract IEnumerator Coroutine(FireTag tag);
	public abstract void Execute(Bullet bullet);
	public abstract YieldInstruction YieldExecute (Bullet Bullet);
	public abstract IEnumerator Coroutine (Bullet Bullet);

	public class Wait : SharedAction
	{
		public override ActionType Type { get { return ActionType.Yield; } }
		
		#if UNITY_EDITOR
		public override void ActionGUI (AttackPattern master)
		{
			wait.EditorGUI ("Time", false);
		}
		
		public override void DrawHandles ()
		{
			//No handles need to be drawn for Wait
		}
		#endif
		
		public override void Execute(FireTag tag)
		{
			throw new InvalidOperationException ();
		}
		
		public override void Execute(Bullet bullet)
		{
			throw new InvalidOperationException ();
		}
		
		public override YieldInstruction YieldExecute(FireTag tag)
		{
			return new WaitForSeconds (wait.Value);
		}
		
		public override YieldInstruction YieldExecute(Bullet bullet)
		{
			return new WaitForSeconds(wait.Value);
		}
		
		public override IEnumerator Coroutine(FireTag tag)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator Coroutine(Bullet bullet)
		{
			throw new InvalidOperationException ();
		}
	}
	
	public class Fire : SharedAction
	{
		public override ActionType Type { get { return ActionType.Normal; } }
		public AttackPattern.Property Angle;
		public AttackPattern.Property Speed;
		
		public int bulletTagIndex;
		public bool useParam = false;
		public bool overwriteBulletSpeed = false;
		public bool useSequenceSpeed = false;
		
		public int fireTagIndex;
		public bool passParam = false;
		public bool passPassedParam = false;
		public Vector2 paramRange;
		
		public DirectionType Direction;
		public SourceType source;
		
		public AudioClip audioClip = null;
		
		#if UNITY_EDITOR
		public override void ActionGUI (AttackPattern master)
		{
			this.master = master;
			Direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", Direction);
			if (!useParam)
			{
				Angle.EditorGUI("Angle", false);
			}
			useParam = EditorGUILayout.Toggle("Use Param Angle", useParam);
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			overwriteBulletSpeed = EditorGUILayout.Toggle("Overwrite Speed", overwriteBulletSpeed);
			if(overwriteBulletSpeed)
			{
				useSequenceSpeed = EditorGUILayout.Toggle("Use Sequence Speed", useSequenceSpeed);
			}
			EditorGUILayout.EndHorizontal();
			if(overwriteBulletSpeed && !useSequenceSpeed)
			{
				Speed.EditorGUI("Speed", false);
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			passParam = EditorGUILayout.Toggle("PassParam", passParam);
			if (!passParam)
			{
				passPassedParam = EditorGUILayout.Toggle("PassMyParam", passPassedParam);
			}
			EditorGUILayout.EndHorizontal();    
			if (passParam)
			{
				paramRange = EditorGUILayout.Vector2Field("Param Range", paramRange);
			}
			bulletTagIndex = EditorUtils.NamedObjectPopup("Bullet Tag", Master.bulletTags, bulletTagIndex, "Bullet Tag");
		}
		
		public override void DrawHandles ()
		{
		}
		#endif
		
		public override void Execute(FireTag tag)
		{

		}
		
		public override void Execute(Bullet bullet)
		{

		}
		
		public override YieldInstruction YieldExecute(FireTag tag)
		{
			throw new InvalidOperationException ();
		}
		
		public override YieldInstruction YieldExecute(Bullet bullet)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator Coroutine(FireTag tag)
		{
			throw new InvalidOperationException ();
		}
		
		public override IEnumerator Coroutine(Bullet bullet)
		{
			throw new InvalidOperationException ();
		}
		
		private void FireBullet(Vector3 position, Quaternion rotation, float param, RotationWrapper previousRotation)
		{
			float angle, direction, angleDifference, speed;
			BulletTag bt = Master.bulletTags[bulletTagIndex];
			Bullet temp = GameObjectManager.Bullets.Get(bt);
			if(previousRotation.rotationNull)
			{
				previousRotation.rotationNull = false;
				previousRotation.rotation = temp.Transform.localRotation;
			}
			
			temp.Transform.position = position;
			temp.Transform.rotation = rotation;
			
			if(useParam)
			{
				angle = param;
			}
			else
			{
				angle = Angle.Value;
			}
			
			switch(Direction)
			{
			case (DirectionType.TargetPlayer):
				Quaternion originalRot = rotation;
				float dotHeading = Vector3.Dot( temp.Transform.up, Player.PlayerTransform.position - temp.Transform.position );
				
				if(dotHeading > 0)
				{
					direction = -1;
				}
				else
				{
					direction = 1;
				}
				angleDifference = Vector3.Angle(temp.Transform.forward, Player.PlayerTransform.position - temp.Transform.position);
				temp.Transform.rotation = originalRot * Quaternion.AngleAxis((direction * angleDifference) - angle, Vector3.right);
				break;
				
			case (DirectionType.Absolute):
				temp.Transform.localRotation = Quaternion.Euler(-(angle - 270), 270, 0);
				break;
				
			case (DirectionType.Relative):
				temp.Transform.localRotation = rotation * Quaternion.AngleAxis (-angle, Vector3.right);
				break;
				
			case (DirectionType.Sequence):
				temp.Transform.localRotation = previousRotation.rotation * Quaternion.AngleAxis (-angle, Vector3.right); 
				break;
			}
			previousRotation.rotation = temp.Transform.localRotation;
			if(overwriteBulletSpeed)
			{
				speed = Speed.Value;
				
				if(useSequenceSpeed)
				{
					Master.sequenceSpeed += speed;
					temp.speed = Master.sequenceSpeed;
				}
				else
				{
					Master.sequenceSpeed = 0.0f;
					temp.speed = speed;
				}
			}
			else
			{	
				temp.speed = bt.speed.Value;
			}
			
			if(passParam)
			{
				temp.param = UnityEngine.Random.Range(paramRange.x, paramRange.y);
			}
			
			if(passPassedParam)
			{
				temp.param = param;
			}
			temp.master = Master;
			temp.GameObject.SetActive(true);
			SoundManager.PlaySoundEffect (audioClip, position);
		}
	}
}
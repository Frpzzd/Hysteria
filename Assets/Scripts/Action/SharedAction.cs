using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class SharedAction : AbstractAction, IFireAction, IBulletAction, IMovementAction, IStageAction
{	
	public class Wait : SharedAction
	{
		public override ActionType Type { get { return ActionType.Yield; } }
		
		public bool WaitForChange;
		public AttackPattern.Property wait;

		#if UNITY_EDITOR
		public override void ActionGUI (params object[] param)
		{
			wait.EditorGUI ("Time", false);
		}
		
		public override void DrawHandles ()
		{
			//No handles need to be drawn for Wait
		}
		#endif
		
		public override void Execute(params object[] param)
		{
			throw new InvalidOperationException ();
		}
		
		public override YieldInstruction YieldExecute(params object[] param)
		{
			return new WaitForSeconds (wait.Value);
		}
		
		public override IEnumerator CoroutineExecute(params object[] param)
		{
			throw new InvalidOperationException ();
		}
	}

	public abstract class Repeat<T, P> : NestedAction<T> where T : Action where P : T, new()
	{
		public override ActionType Type { get { return ActionType.Normal; } }
		public AttackPattern.Property repeat;
		
		#if UNITY_EDITOR
		public override void ActionGUI (params object[] param)
		{
			if (nestedActions == null || nestedActions.Length == 0)
			{
				nestedActions = new T[1];
				nestedActions [0] = new P();
			}

			repeat.EditorGUI ("Repeat", true);

			nestedActions = EditorUtils.ActionGUI<T, P>(nestedActions, false, param);
		}

		private T[] Convert(Action[] array)
		{
			T[] temp = new T[array.Length];
			for(int i = 0; i < temp.Length; i++)
			{
				temp[i] = (T)array[i];
			}
			return temp;
		}

		private Action[] Convert(T[] array)
		{
			Action[] temp = new Action[array.Length];
			for(int i = 0; i < temp.Length; i++)
			{
				temp[i] = (T)array[i];
			}
			return temp;
		}
		
		public override void DrawHandles ()
		{
			//TO-DO
		}
		
		public override void Expand(bool recursive)
		{
			SetAll (true, recursive);
		}
		
		public override void Collapse(bool recursive)
		{
			SetAll (false, recursive);
		}
		
		public override void SetAll(bool value, bool recursive)
		{
			Foldout = value;
			if(recursive && nestedActions != null && nestedActions.Length > 0)
			{
				for(int i = 0; i < nestedActions.Length; i++)
				{
					if(nestedActions[i] is NestedAction<T>)
					{
						(nestedActions[i] as NestedAction<T>).SetAll(value, recursive);
					}
				}
			}
		}
		#endif
		public override void Execute (params object[] param)
		{			
			for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
			{
				for(int i = 0; i < nestedActions.Length; i++)
				{
					ActionExecutor.ExecuteAction(nestedActions[i], param);
				}
			}
		}
		
		public override IEnumerator CoroutineExecute (params object[] param)
		{
			throw new InvalidOperationException ();
		}
		
		public override YieldInstruction YieldExecute (params object[] param)
		{
			throw new NotImplementedException ();
		}
	}
}


public class Fire : AttackPatternAction, IFireAction, IBulletAction
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
	
	public override void Execute(params object[] param)
	{
		
	}
	public override YieldInstruction YieldExecute(params object[] param)
	{
		throw new InvalidOperationException ();
	}
	
	public override IEnumerator CoroutineExecute(params object[] param)
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
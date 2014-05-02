using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DanmakuEngine.Actions
{
//	public static class ActionHandler
//	{
//		#if UNITY_EDITOR
//		public static void DrawActionGizmo<T, P>(T action, T previous, Color gizmoColor) where T : NestedAction<T, P> where P : struct, IConvertible
//		{
//			action.DrawGizmos(previous);
//		}
//		
//		public static void DrawActionGizmos<T, P>(T[] actions, Color groupColor) where T : NestedAction<T, P> where P : struct, IConvertible
//		{
//			if(actions != null)
//			{
//				for(int i = 0; i < actions.Length; i++)
//				{
//					actions[i].DrawGizmos((i == 0) ? null : actions[i - 1]);
//				}
//			}
//		}
//		#endif
//	}

	[Serializable]
	public abstract class Action
	{
		[NonSerialized]
		public bool foldout = true;
		
		[NonSerialized]
		public bool Initialized = false;
		
		[NonSerialized]
		public MonoBehaviour parent;
		
		[SerializeField]
		public string representation;
		
		[SerializeField]
		[DanmakuProperty("Total Time", false, true)]
		public DanmakuProperty wait;
		
		public virtual void Initialize(MonoBehaviour parent)
		{
			this.parent = parent;
			Initialized = true;
		}
		
		public abstract IEnumerator Execute(params object[] param);
	}
	
	[Serializable]
	public abstract class NestedAction<T, P> : Action where T : NestedAction<T, P> where P : struct, IConvertible
	{
		public T[] nestedActions;
		public P type;
		[DanmakuProperty("Count", true, true)]
		public DanmakuProperty repeat;
		public bool usingRepeat;
		
		public bool drawHandles = true;

		public override void Initialize (MonoBehaviour parent)
		{
			base.Initialize (parent);
			foreach(T action in nestedActions)
			{
				action.Initialize(parent);
			}
		}

		public void DrawHandles(T previous, Color handleColor)
		{
			#if UNITY_EDITOR
			//if(drawHandles)
			//{
				Color oldColor = Handles.color;
				Handles.color = handleColor;
				DrawHandlesImpl(previous);
				Handles.color = oldColor;
			//}
			#endif
		}
		
		protected abstract void DrawHandlesImpl (T previous);
		
		public override string ToString ()
		{
			return type.ToString ();
		}

		protected static void RepeatHandles(T nestedAction, Color handleColor)
		{
			if(nestedAction.nestedActions != null)
			{
				for(int j = 0; j < Mathf.FloorToInt(nestedAction.repeat.Value); j++)
				{
					for(int i = 0; i < nestedAction.nestedActions.Length; i++)
					{
						if(i == 0)
						{
							if(j == 0)
							{
								nestedAction.nestedActions[i].DrawHandles(null, handleColor);
							}
							else
							{
								nestedAction.nestedActions[i].DrawHandles(nestedAction.nestedActions[nestedAction.nestedActions.Length - 1], handleColor);
							}
						}
						else
						{
							nestedAction.nestedActions[i].DrawHandles(nestedAction.nestedActions[i - 1], handleColor);
						}
					}
				}
			}
		}
	}
}
using UnityEngine;
using System;
using System.Collections;
using UnityEditor;

namespace DanmakuEngine.Actions
{
	
	[Serializable]
	public class MovementAction : NestedAction<MovementAction, MovementAction.Type>
	{
		public enum Type { Wait, Repeat, Linear, Curve }
		public enum LocationType { Absolute, Relative }

		private static Texture2D curveTex;
		static MovementAction()
		{
			curveTex = new Texture2D (1, 1);
			curveTex.SetPixel (0, 0, Color.white);
		}
		
		[SerializeField]
		public Vector2 targetLocation;
		[SerializeField]
		public LocationType locationType;
		[SerializeField]
		public Vector2 control1;
		[SerializeField]
		public Vector2 control2;

		protected override void DrawHandlesImpl (MovementAction previous)
		{
		}
		
		public Vector3 DrawHandles(Vector3 previousPosition, bool mirrorMovementX, bool mirrorMovementY, Color handlesColor)
		{
			Color oldColor = Handles.color;
			Handles.color = handlesColor;
			Vector3 actualLocation = previousPosition;
			if(type != Type.Wait)
			{
				Vector3 target = targetLocation;
				Vector3 c1 = new Vector3(control1.x, control1.y);
				Vector3 c2 = new Vector3(control2.x, control2.y);
				if(mirrorMovementX)
				{
					target.x *= -1;
					c1.x *= -1;
					c2.x *= -1;
				}
				if(mirrorMovementY)
				{
					target.y *= -1;
					c1.y *= -1;
					c2.y *= -1;
				}
				if(type == Type.Repeat)
				{
					for(int j = 0; j < Mathf.FloorToInt(repeat.Value); j++)
					{
						Vector3 temp = actualLocation;
	
						for(int i = 0; i < nestedActions.Length; i++)
						{
							actualLocation = nestedActions[i].DrawHandles(actualLocation, mirrorMovementX, mirrorMovementY, handlesColor);
						}
	
						if(temp == actualLocation)
						{
							break; //Loop doesn't change, just draw once
						}
					}
				}
				else 
				{
					if(locationType == LocationType.Relative)
					{
						actualLocation = previousPosition + target;
						c1 += previousPosition;
						c2 += previousPosition;
					}
					else
					{
						actualLocation = target;
					}
					if(type == Type.Curve)
					{
						Handles.color = Color.white;
						Handles.DrawLine(previousPosition, c1);
						Handles.DrawWireDisc(c1, Vector3.forward, 1);
						Handles.DrawLine(c1, c2);
						Handles.DrawWireDisc(c2, Vector3.forward, 1);
						Handles.DrawLine(c2, actualLocation);
						Handles.DrawBezier(previousPosition, actualLocation, c1, c2, handlesColor, curveTex, 0.1f);
						Handles.color = handlesColor;
					}
					else if (type == Type.Linear)
					{
						Handles.DrawLine(previousPosition, actualLocation);
					}
					Handles.DrawWireDisc(actualLocation, Vector3.forward, 1);
				}
			}
			Handles.color = oldColor;
			return actualLocation;
		}
		
		public override IEnumerator Execute (params object[] param)
		{
			Transform transform = param [0] as Transform;
			ActionAttackPattern attackPattern = param [1] as ActionAttackPattern;
			bool mirrorMovementX = (bool)param [2];
			bool mirrorMovementY = (bool)param [3];
			Vector3 start = Vector3.zero, end = Vector3.zero, c1 = Vector3.zero, c2 = Vector3.zero;
			float totalTime = wait.Value;
			float deltat = Time.fixedDeltaTime;
			IEnumerator pause, actionEnumerator;
			float lerpValue = 0f;
			if(attackPattern.currentHealth < 0)
			{
				return false;
			}
			switch(type)
			{
			case Type.Linear:
			case Type.Curve:
				start = transform.position;
				end = targetLocation;
				c1 = new Vector3(control1.x, control1.y);
				c2 = new Vector3(control2.x, control2.y);
				if(mirrorMovementX)
				{
					end.x *= -1;
					c1.x *= -1;
					c2.x *= -1;
				}
				if(mirrorMovementY)
				{
					end.y *= -1;
					c1.y *= -1;
					c2.y *= -1;
				}
				if(locationType == LocationType.Relative)
				{
					end += start;
					c1 += start;
					c2 += start;
				}
				break;
			}
			switch(type)
			{
			case Type.Linear:
				lerpValue = 0f;
				while(lerpValue <= 1f)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
					transform.position = Vector3.Lerp(start, end, lerpValue);
					yield return new WaitForFixedUpdate();
					lerpValue +=  deltat / totalTime;
				}
				transform.position = end;
				break;
			case Type.Curve:
				float u, uu, uuu, t = 0f, tt, ttt;
				Vector3 p, p0 = start, p1 = c1, p2 = c2, p3 = end;
				while(t <= 1f)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
					u = 1 - t;
					uu = u*u;
					uuu = uu * u;
					tt = t * t;
					ttt = tt * t;

					p = uuu * p0; //first term
					p += 3 * uu * t * p1; //second term
					p += 3 * u * tt * p2; //third term
					p += ttt * p3; //fourth term
					transform.position = p;
					yield return new WaitForFixedUpdate();
					t +=  deltat / totalTime;
				}
				transform.position = end;
				break;
			case Type.Repeat:
				int repeatC = Mathf.FloorToInt(repeat.Value);
				for(int j = 0; j < repeatC; j++)
				{
					foreach(MovementAction action in nestedActions)
					{
						pause = Global.WaitForUnpause();
						while(pause.MoveNext())
						{
							yield return pause.Current;
						}
						if(attackPattern.currentHealth < 0)
						{
							return false;
						}
						actionEnumerator = action.Execute(param[0], param[1], param[2]);
						while(actionEnumerator.MoveNext())
						{
							yield return actionEnumerator.Current;
						}
					}
				}
				break;
			case Type.Wait:
				float currentTime = 0f;
				totalTime = wait.Value;
				while(currentTime < totalTime)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
					if(attackPattern.currentHealth <= 0)
					{
						return false;
					}
					yield return new WaitForFixedUpdate();
					currentTime += deltat;
				}
				break;
			}
		}
	}

}
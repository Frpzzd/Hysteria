using UnityEngine;
using System;
using System.Collections;
using DanmakuEngine.Core;

public interface NamedObject
{
	string Name { get; set; }
}

public interface TitledObject
{
	string Title { get; set; }
}

namespace DanmakuEngine.Actions
{
	/// <summary>
	/// Fire Tag
	/// A group of actions that describe a firing pattern used in a AttackPattern
	/// </summary>
	[System.Serializable]
	public class FireTag : NamedObject
	{
		[SerializeField]
		private string name = "Fire Tag";
		
		/// <summary>
		/// The actions executed by this FireTag
		/// </summary>
		[SerializeField]
		public FireAction[] actions;
		
		/// <summary>
		/// Whether this FireTag is start when the AttackPattern starts
		/// </summary>
		[SerializeField]
		public bool runAtStart = true;
		
		/// <summary>
		/// If this value is true, this FireTag will loop until the AttackPattern this FireTag belongs
		/// to ends.
		/// </summary>
		[SerializeField]
		public bool loopUntilEnd = true;
		
		/// <summary>
		/// If this tag loops until the end of the AttackPattern, this AttackPattern Property determines
		/// how long it waits between each loop
		/// </summary>
		[SerializeField]
		[DanmakuProperty("Loop Wait Time", false, false)]
		public DanmakuProperty wait;
		
		/// <summary>
		/// Gets or sets the name of the FireTag
		/// This is mainly used by the Editor to differentiate between different FireTags
		/// and has no actual impact on the execution of game.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return name;
			}
			
			set
			{
				name = value;
			}
		}
		
		public Rank minimumRank = Rank.Easy;
		
		/// <summary>
		/// Executes the FireTag as specified 
		/// </summary>
		/// <param name="param">Parameter.</param>
		public IEnumerator Run (params object[] param)
		{
			if(Global.Rank < minimumRank)
			{
				return false;
			}
			AbstractEnemy enemy = param [0] as AbstractEnemy;
			ActionAttackPattern pattern = param [1] as ActionAttackPattern;
			float floatParam = (param.Length > 2) ? (float)param [2] : 0.0f;
			SequenceWrapper prevRotation = new SequenceWrapper ();
			IEnumerator pause, actionEnumerator;
			if(actions != null && actions.Length > 0)
			{
				if(loopUntilEnd)
				{
					while(pattern.currentHealth > 0)
					{
						foreach(Action action in actions)
						{
							pause = Global.WaitForUnpause();
							while(pause.MoveNext())
							{
								yield return pause.Current;
							}
							actionEnumerator = action.Execute(enemy, pattern, floatParam, prevRotation);
							while(actionEnumerator.MoveNext())
							{
								yield return actionEnumerator.Current;
							}
						}
						float waitC = wait.Value;
						if(waitC <= Time.fixedDeltaTime)
						{
							yield return new WaitForFixedUpdate();
						}
						else
						{
							float currentTime = 0f;
							while(currentTime < waitC)
							{
								pause = Global.WaitForUnpause();
								while(pause.MoveNext())
								{
									yield return pause.Current;
								}
								yield return new WaitForFixedUpdate();
								currentTime += Time.fixedDeltaTime;
							}
						}
					}
				}
				else
				{
					foreach(Action action in actions)
					{
						pause = Global.WaitForUnpause();
						while(pause.MoveNext())
						{
							yield return pause.Current;
						}
						actionEnumerator = action.Execute(enemy, pattern, floatParam, prevRotation);
						while(actionEnumerator.MoveNext())
						{
							yield return actionEnumerator.Current;
						}
					}
				}
			}
		}
		
		public void Initialize(MonoBehaviour parent)
		{
			foreach(FireAction action in actions)
			{
				action.Initialize(parent);
			}
		}
	}
	
	[System.Serializable]
	public class BulletTag : NamedObject
	{
		[SerializeField]
		private string name = "Bullet Tag";
		[SerializeField]
		[DanmakuProperty("Bullet Speed", false, false)]
		public DanmakuProperty speed;
		[SerializeField]
		public GameObject prefab;
		[SerializeField]
		public Bullet.Action[] actions;
		[SerializeField]
		public bool overwriteColor = false;
		[SerializeField]
		public Color newColor = Color.white;
		
		public string Name
		{
			get
			{
				return name;
			}
			
			set
			{
				name = value;
			}
		}
		
		public BulletTag()
		{
			actions = new Bullet.Action[0];
		}
		
		public IEnumerator Run (params object[] param)
		{
			if(actions != null && actions.Length > 0)
			{
				IEnumerator pause, actionEnumerator;
				foreach(Action action in actions)
				{
					pause = Global.WaitForUnpause();
					while(pause.MoveNext())
					{
						yield return pause.Current;
					}
					actionEnumerator = action.Execute(param[0] as Bullet, param[1] as ActionAttackPattern);
					while(actionEnumerator.MoveNext())
					{
						yield return actionEnumerator.Current;
					}
				}
			}
		}
		
		public void Initialize(MonoBehaviour parent)
		{
			foreach(Bullet.Action action in actions)
			{
				action.Initialize(parent);
			}
		}
	}
}
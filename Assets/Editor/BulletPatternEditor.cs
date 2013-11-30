using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BulletPattern))]
public class BulletPatternEditor : Editor 
{
	BulletPattern bp;
	
	void OnEnable()
	{
		bp = target as BulletPattern;
	}
	
	public override void OnInspectorGUI() 
	{
		bp.bossPattern = EditorGUILayout.Toggle ("Boss Pattern", bp.bossPattern);
		if(bp.bossPattern)
		{
			EditorGUI.indentLevel++;
			bp.bpName = EditorGUILayout.TextField ("Name", bp.bpName);
			bp.survival = EditorGUILayout.Toggle ("Survival", bp.survival);
			bp.maxHealth = EditorGUILayout.IntField ("Health", bp.maxHealth);
			bp.timeOut = EditorGUILayout.IntField ("Time", bp.timeOut);
			bp.bonus = EditorGUILayout.IntField ("Bonus", bp.bonus);
			bp.bonusPerSecond = EditorGUILayout.IntField("Bonus Points Per Second", bp.bonusPerSecond);
			EditorGUI.indentLevel--;
		}
		FireTagsGUI ();
		BulletTagsGUI ();
		
		EditorGUILayout.Space();
		EditorGUIUtility.labelWidth = 260;
		bp.waitBeforeRepeating = EditorGUILayout.FloatField("WaitBeforeRepeat", bp.waitBeforeRepeating);
		Global.Rank = (int)EditorGUILayout.Slider("Rank", Global.Rank,0,1);
		
	}
	
	private void FireTagsGUI () 
	{
		List<FireTag> fireTags;

		if (bp.fireTags == null)
			bp.fireTags = new FireTag[0];
		
		fireTags = new List<FireTag>(bp.fireTags);
		
		if (fireTags.Count != bp.ftFoldouts.Count)
			bp.ftFoldouts = new List<bool>(new bool[fireTags.Count]);
		
		if (fireTags.Count != bp.ftaFoldouts.Count)
		{
			bp.ftaFoldouts = new List<ActionFoldouts>(new ActionFoldouts[fireTags.Count]);
			if(bp.ftaFoldouts.Count > 0)
			{
				for(int zz = 0; zz < bp.ftaFoldouts.Count;zz++)
				{
					bp.ftaFoldouts[zz] = new ActionFoldouts();
				}
			}
		}
		
		GUILayout.BeginHorizontal();
		bp.ftFoldout = EditorGUILayout.Foldout(bp.ftFoldout, "FireTags");
		if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
		{
			bp.ftFoldout = !bp.ftFoldout;
			
			for(int zz = 0; zz < bp.ftFoldouts.Count;zz++)
				bp.ftFoldouts[zz] = bp.ftFoldout;
		}
		GUILayout.EndHorizontal();
		
		if (bp.ftFoldout) 
		{
			EditorGUI.indentLevel++;
			int removeIndex = -1;
			int moveIndex = -1;
			
			for (int l=0; l<fireTags.Count; l++) 
			{
				GUILayout.BeginHorizontal();
				string str = "FireTag " + (l+1);
				bp.ftFoldouts[l] = EditorGUILayout.Foldout(bp.ftFoldouts[l], str);
				
				if (GUILayout.Button("Down", GUILayout.Width(50)))
					moveIndex = l;
				
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
					removeIndex = l;
				GUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				
				if (bp.ftFoldouts[l]) 
				{
					GUI.changed = false;
					
					EditorGUI.indentLevel++;
					
					if (GUI.changed) 
						SceneView.RepaintAll();
					
					FireTagActionsGUI(l);
					
					EditorGUI.indentLevel--;
					EditorGUILayout.Space();
				}
			}
			
			// if the "down" button was pressed then we move that array index down one time, MAGIC
			if(moveIndex >= 0 && moveIndex != fireTags.Count-1)
			{
				FireTag temp = fireTags[moveIndex];	
				fireTags[moveIndex] = fireTags[moveIndex+1];
				fireTags[moveIndex+1] = temp;
				
				bool temp2 = bp.ftFoldouts[moveIndex];
				bp.ftFoldouts[moveIndex] = bp.ftFoldouts[moveIndex+1];
				bp.ftFoldouts[moveIndex+1] = temp2;
				
				ActionFoldouts temp3 = bp.ftaFoldouts[moveIndex];
				bp.ftaFoldouts[moveIndex] = bp.ftaFoldouts[moveIndex+1];
				bp.ftaFoldouts[moveIndex+1] = temp3;
			}
			// hmm what could remove do
			if (removeIndex >= 0) 
			{
				fireTags.RemoveAt(removeIndex);
				bp.ftFoldouts.RemoveAt(removeIndex);
				bp.ftaFoldouts.RemoveAt(removeIndex);
			}
			
			//add a space to the GUI, adding a number in those paranthesis(brackets to you brits) will increase the space size
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("");
			if (GUILayout.Button("Add Fire Tag", GUILayout.Width(100))) 
			{
				FireTag ft = new FireTag();
				ft.actions = new FireAction[1];
				ft.actions[0] = new FireAction();
				
				fireTags.Add(ft);
				bp.ftFoldouts.Add(true);
				bp.ftaFoldouts.Add(new ActionFoldouts());
				
			}
			GUILayout.EndHorizontal();
			
			bp.fireTags = fireTags.ToArray();
			
			EditorGUI.indentLevel--;
			
		}
		
		EditorGUILayout.Space();
	}
	
	//start the FireActions stuff. Its actually even longer and uglier then the previous function
	void FireTagActionsGUI(int i)
	{
		if (bp.fireTags[i].actions.Length == 0)
			bp.fireTags[i].actions = new FireAction[1];
		
		List<FireAction> actions = new List<FireAction>(bp.fireTags[i].actions);
		
		if (actions.Count != bp.ftaFoldouts[i].sub.Count)
			bp.ftaFoldouts[i].sub = new List<bool>(new bool[actions.Count]);
		
		GUILayout.BeginHorizontal();
		bp.ftaFoldouts[i].main = EditorGUILayout.Foldout(bp.ftaFoldouts[i].main, "Actions");
		if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
		{
			bp.ftaFoldouts[i].main = !bp.ftaFoldouts[i].main;
			
			for(int zz = 0; zz < bp.ftaFoldouts[i].sub.Count;zz++)
				bp.ftaFoldouts[i].sub[zz] = bp.ftaFoldouts[i].main;		
		}
		GUILayout.EndHorizontal();
		
		if (bp.ftaFoldouts[i].main ) 
		{
			EditorGUI.indentLevel++;
			int removeIndex = -1;
			int moveIndex = -1;
			
			for (int l=0; l<actions.Count; l++) 
			{
				GUILayout.BeginHorizontal();
				string str = "Action " + (l+1);
				bp.ftaFoldouts[i].sub[l] = EditorGUILayout.Foldout(bp.ftaFoldouts[i].sub[l], str);
				
				if (GUILayout.Button("Down", GUILayout.Width(50)))
					moveIndex = l;
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
					removeIndex = l;
				GUILayout.EndHorizontal();
				
				if (bp.ftaFoldouts[i].sub[l]) 
				{
					GUI.changed = false;
					
					EditorGUI.indentLevel++;
					
					FireAction ac = actions[l];
					
					ac.type = (FireActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);
					
					//an extremely ugly block of GUI code
					switch(ac.type)
					{
					case(FireActionType.Wait):
						GUILayout.BeginHorizontal();
						if(!ac.randomWait)
							ac.waitTime.x = EditorGUILayout.FloatField("Wait Time", ac.waitTime.x);
						else
							ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
						ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankWait = EditorGUILayout.Toggle("Add Rank", ac.rankWait);
						if(ac.rankWait)
							ac.waitTime.z = EditorGUILayout.FloatField("RankWaitTime", ac.waitTime.z);
						GUILayout.EndHorizontal();	
						break;
						
					case(FireActionType.Fire):
						ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
						if(!ac.useParam)
						{
							GUILayout.BeginHorizontal();
							if(!ac.randomAngle)
								ac.angle.x = EditorGUILayout.FloatField("Angle", ac.angle.x);
							else
								ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
							ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
							if(ac.rankAngle)
								ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
							GUILayout.EndHorizontal();	
						}
						ac.useParam = EditorGUILayout.Toggle("Use Param", ac.useParam);
						EditorGUILayout.Space();
						
						ac.overwriteBulletSpeed = EditorGUILayout.Toggle("OverwriteSpd", ac.overwriteBulletSpeed);
						if(ac.overwriteBulletSpeed)
						{
							GUILayout.BeginHorizontal();
							if(!ac.randomSpeed)
								ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
							else
								ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
							ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
							if(ac.rankSpeed)
								ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
							GUILayout.EndHorizontal();	
							ac.useSequenceSpeed = EditorGUILayout.Toggle("UseSequence", ac.useSequenceSpeed);
						}
						
						EditorGUILayout.Space();
						GUILayout.BeginHorizontal();
						ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
						if(!ac.passParam)
							ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
						GUILayout.EndHorizontal();	
						if(ac.passParam)
							ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
						if(bp.bulletTags.Length > 1)
							ac.bulletTagIndex = EditorGUILayout.IntSlider("BulletTag Index", ac.bulletTagIndex, 1, bp.bulletTags.Length);
						break;
						
					case(FireActionType.CallFireTag):
						if(bp.fireTags.Length > 1)
							ac.fireTagIndex = EditorGUILayout.IntSlider("Fire Tag Idx", ac.fireTagIndex, 1, bp.fireTags.Length);
						GUILayout.BeginHorizontal();
						ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
						if(!ac.passParam)
							ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
						GUILayout.EndHorizontal();	
						if(ac.passParam)
							ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
						break;
						
					case(FireActionType.StartRepeat):
						ac.repeatCount.x = (int)EditorGUILayout.IntField("RepeatCount", (int)ac.repeatCount.x);
						GUILayout.BeginHorizontal();
						ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
						if(ac.rankRepeat)
							ac.repeatCount.y = EditorGUILayout.FloatField("RankRepeat", ac.repeatCount.y);
						GUILayout.EndHorizontal();
						break;
					}
					EditorGUI.indentLevel--;
					if (GUI.changed) 
						SceneView.RepaintAll();
				}
			}
			
			if(moveIndex >= 0 && moveIndex != actions.Count-1)
			{
				FireAction temp = actions[moveIndex];	
				actions[moveIndex] = actions[moveIndex+1];
				actions[moveIndex+1] = temp;
				
				bool temp2 = bp.ftaFoldouts[i].sub[moveIndex];
				bp.ftaFoldouts[i].sub[moveIndex] = bp.ftaFoldouts[i].sub[moveIndex+1];
				bp.ftaFoldouts[i].sub[moveIndex+1] = temp2;
			}
			// Ive seen this before somewhere
			if (removeIndex >= 0) 
			{
				actions.RemoveAt(removeIndex);
				bp.ftaFoldouts[i].sub.RemoveAt(removeIndex);
			}
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("");
			if (GUILayout.Button("Add Action", GUILayout.Width(100))) 
			{
				FireAction ac = new FireAction();
				actions.Add(ac);
				bp.ftaFoldouts[i].sub.Add(true);
			}
			GUILayout.EndHorizontal();
			
			bp.fireTags[i].actions = actions.ToArray();	
			EditorGUI.indentLevel--;
		}
	}
	
	//BulletTag stuff, somewhat shorter than the last one
	public void BulletTagsGUI () 
	{
		if (bp.bulletTags == null)
			bp.bulletTags = new BulletTag[0];
		
		List<BulletTag> bulletTags = new List<BulletTag>(bp.bulletTags);
		
		if (bulletTags.Count != bp.btFoldouts.Count)
			bp.btFoldouts = new List<bool>(new bool[bulletTags.Count]);
		
		if (bulletTags.Count != bp.btaFoldouts.Count)
		{
			bp.btaFoldouts = new List<ActionFoldouts>(new ActionFoldouts[bulletTags.Count]);
			if(bp.btaFoldouts.Count > 0)
			{
				for(int zz = 0; zz < bp.btaFoldouts.Count;zz++)
				{
					bp.btaFoldouts[zz] = new ActionFoldouts();
				}
			}
		}
		
		GUILayout.BeginHorizontal();
		bp.btFoldout = EditorGUILayout.Foldout(bp.btFoldout, "BulletTags");
		if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
		{
			bp.btFoldout = !bp.btFoldout;
			
			for(int zz = 0; zz < bp.btFoldouts.Count;zz++)
				bp.btFoldouts[zz] = bp.btFoldout;	
		}
		GUILayout.EndHorizontal();
		
		if (bp.btFoldout) 
		{
			EditorGUI.indentLevel++;
			int removeIndex = -1;
			int moveIndex = -1;
			
			for (int l=0; l<bulletTags.Count; l++) 
			{
				GUILayout.BeginHorizontal();
				string str = "BulletTag " + (l+1);
				bp.btFoldouts[l] = EditorGUILayout.Foldout(bp.btFoldouts[l], str);
				
				if (GUILayout.Button("Down", GUILayout.Width(50)))
					moveIndex = l;
				
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
					removeIndex = l;
				GUILayout.EndHorizontal();
				
				EditorGUILayout.Space();
				
				if (bp.btFoldouts[l]) 
				{
					GUI.changed = false;
					
					EditorGUI.indentLevel++;
					
					BulletTag bt = bulletTags[l];
					
					GUILayout.BeginHorizontal();
					if(!bt.randomSpeed)
						bt.speed.x = EditorGUILayout.FloatField("Speed", bt.speed.x);
					else
						bt.speed = EditorGUILayout.Vector2Field("Speed Range", bt.speed);
					bt.randomSpeed = EditorGUILayout.Toggle("Randomize", bt.randomSpeed);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					bt.rankSpeed = EditorGUILayout.Toggle("Add Rank", bt.rankSpeed);
					if(bt.rankSpeed)
						bt.speed.z = EditorGUILayout.FloatField("RankSpeed", bt.speed.z);
					GUILayout.EndHorizontal();
					EditorGUILayout.ObjectField("Bullet Prefab ", bt.prefab, typeof(GameObject), false);
					
					if (GUI.changed) 
						SceneView.RepaintAll();
					
					BulletTagActionsGUI(l);
					
					EditorGUI.indentLevel--;
					EditorGUILayout.Space();
				}
			}
			
			if(moveIndex >= 0 && moveIndex != bulletTags.Count-1)
			{
				BulletTag temp = bulletTags[moveIndex];	
				bulletTags[moveIndex] = bulletTags[moveIndex+1];
				bulletTags[moveIndex+1] = temp;
				
				bool temp2 = bp.btFoldouts[moveIndex];
				bp.btFoldouts[moveIndex] = bp.btFoldouts[moveIndex+1];
				bp.btFoldouts[moveIndex+1]= temp2;
				
				ActionFoldouts temp3 = bp.btaFoldouts[moveIndex];
				bp.btaFoldouts[moveIndex] = bp.btaFoldouts[moveIndex+1];
				bp.btaFoldouts[moveIndex+1] = temp3;
			}
			
			if (removeIndex >= 0) 
			{
				bulletTags.RemoveAt(removeIndex);
				bp.btFoldouts.RemoveAt(removeIndex);
				bp.btaFoldouts.RemoveAt(removeIndex);
			}
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("");
			if (GUILayout.Button("Add Bullet Tag", GUILayout.Width(100))) 
			{
				BulletTag bt = new BulletTag();
				bulletTags.Add(bt);
				bp.btFoldouts.Add(true);
				bp.btaFoldouts.Add(new ActionFoldouts());
				
			}
			GUILayout.EndHorizontal();
			
			bp.bulletTags = bulletTags.ToArray();
			
		}
		
		EditorGUILayout.Space();
	}
	
	//obligatory comment that makes this function stand out so you can find it better
	void BulletTagActionsGUI(int i)
	{
		if (bp.bulletTags[i].actions == null)
			bp.bulletTags[i].actions = new BulletAction[0];
		
		List<BulletAction> actions = new List<BulletAction>(bp.bulletTags[i].actions);
		
		if (actions.Count != bp.btaFoldouts[i].sub.Count)
			bp.btaFoldouts[i].sub = new List<bool>(new bool[actions.Count]);
		
		GUILayout.BeginHorizontal();
		bp.btaFoldouts[i].main = EditorGUILayout.Foldout(bp.btaFoldouts[i].main, "Actions");
		if (GUILayout.Button("Collapse All", GUILayout.Width(150)))
		{
			bp.btaFoldouts[i].main = !bp.btaFoldouts[i].main;
			
			for(int zz = 0; zz < bp.btaFoldouts[i].sub.Count;zz++)
				bp.btaFoldouts[i].sub[zz] = bp.btaFoldouts[i].main;		
		}
		GUILayout.EndHorizontal();
		
		if (bp.btaFoldouts[i].main ) 
		{
			EditorGUI.indentLevel++;
			int removeIndex = -1;
			int moveIndex = -1;
			
			for (int l=0; l<actions.Count; l++) 
			{
				GUILayout.BeginHorizontal();
				string str = "Action " + (l+1);
				bp.btaFoldouts[i].sub[l] = EditorGUILayout.Foldout(bp.btaFoldouts[i].sub[l], str);
				
				if (GUILayout.Button("Down", GUILayout.Width(50)))
					moveIndex = l;
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
					removeIndex = l;
				GUILayout.EndHorizontal();
				
				if (bp.btaFoldouts[i].sub[l]) 
				{
					GUI.changed = false;
					
					EditorGUI.indentLevel++;
					
					BulletAction ac = actions[l];
					
					ac.type = (BulletActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);
					
					switch(ac.type)
					{
					case(BulletActionType.Wait):
						GUILayout.BeginHorizontal();
						if(!ac.randomWait)
							ac.waitTime.x = EditorGUILayout.FloatField("Wait Time", ac.waitTime.x);
						else
							ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
						ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankWait = EditorGUILayout.Toggle("AddRank", ac.rankWait);
						if(ac.rankWait)
							ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
						GUILayout.EndHorizontal();
						break;
						
					case(BulletActionType.Change_Direction):
						ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
						GUILayout.BeginHorizontal();
						if(!ac.randomAngle)
							ac.angle.x = EditorGUILayout.FloatField("Angle", ac.angle.x);
						else
							ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
						ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
						if(ac.rankAngle)
							ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
						GUILayout.EndHorizontal();	
						
						GUILayout.BeginHorizontal();
						if(!ac.randomWait)
							ac.waitTime.x = EditorGUILayout.FloatField("Time", ac.waitTime.x);
						else
							ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
						ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankWait = EditorGUILayout.Toggle("AddRank", ac.rankWait);
						if(ac.rankWait)
							ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
						GUILayout.EndHorizontal();
						
						ac.waitForChange = EditorGUILayout.Toggle("WaitToFinish", ac.waitForChange);
						break;
						
					case(BulletActionType.Change_Speed):
					case(BulletActionType.Vertical_Change_Speed):
						GUILayout.BeginHorizontal();
						if(!ac.randomSpeed)
							ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
						else
							ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
						ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
						if(ac.rankSpeed)
							ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
						GUILayout.EndHorizontal();	
						EditorGUILayout.Space();
						GUILayout.BeginHorizontal();
						if(!ac.randomWait)
							ac.waitTime.x = EditorGUILayout.FloatField("Time", ac.waitTime.x);
						else
							ac.waitTime = EditorGUILayout.Vector2Field("Time Range", ac.waitTime);
						ac.randomWait = EditorGUILayout.Toggle("Randomize", ac.randomWait);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						ac.rankWait = EditorGUILayout.Toggle("Add Rank", ac.rankWait);
						if(ac.rankWait)
							ac.waitTime.z = EditorGUILayout.FloatField("RankTime", ac.waitTime.z);
						GUILayout.EndHorizontal();
						
						ac.waitForChange = EditorGUILayout.Toggle("WaitToFinish", ac.waitForChange);
						break;
						
					case(BulletActionType.Start_Repeat):
						ac.repeatCount.x = (int)EditorGUILayout.IntField("Repeat Count", (int)ac.repeatCount.x);
						GUILayout.BeginHorizontal();
						ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
						if(ac.rankRepeat)
							ac.repeatCount.y = EditorGUILayout.FloatField("RepeatRank", ac.repeatCount.y);
						GUILayout.EndHorizontal();
						break;
						
					case(BulletActionType.Fire):	
						ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
						
						if(!ac.useParam)
						{	
							GUILayout.BeginHorizontal();
							if(!ac.randomAngle)
								ac.angle.x = EditorGUILayout.FloatField("Angle", ac.angle.x);
							else
								ac.angle = EditorGUILayout.Vector2Field("Angle Range", ac.angle);
							ac.randomAngle = EditorGUILayout.Toggle("Randomize", ac.randomAngle);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							ac.rankAngle = EditorGUILayout.Toggle("Add Rank", ac.rankAngle);
							if(ac.rankAngle)
								ac.angle.z = EditorGUILayout.FloatField("RankAngle", ac.angle.z);
							GUILayout.EndHorizontal();	
						}
						ac.useParam = EditorGUILayout.Toggle("UseParamAngle", ac.useParam);
						EditorGUILayout.Space();
						ac.overwriteBulletSpeed = EditorGUILayout.Toggle("OverwriteSpeed", ac.overwriteBulletSpeed);
						if(ac.overwriteBulletSpeed)
						{
							GUILayout.BeginHorizontal();
							if(!ac.randomSpeed)
								ac.speed.x = EditorGUILayout.FloatField("New Speed", ac.speed.x);
							else
								ac.speed = EditorGUILayout.Vector2Field("Speed Range", ac.speed);
							ac.randomSpeed = EditorGUILayout.Toggle("Randomize", ac.randomSpeed);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							ac.rankSpeed = EditorGUILayout.Toggle("Add Rank", ac.rankSpeed);
							if(ac.rankSpeed)
								ac.speed.z = EditorGUILayout.FloatField("RankSpeed", ac.speed.z);
							GUILayout.EndHorizontal();	
							ac.useSequenceSpeed = EditorGUILayout.Toggle("UseSequence", ac.useSequenceSpeed);
						}
						EditorGUILayout.Space();
						ac.bulletTagIndex = EditorGUILayout.IntSlider("BulletTagIdx", ac.bulletTagIndex, 1, bp.bulletTags.Length);
						break;
					}
					EditorGUI.indentLevel--;
					if (GUI.changed) 
						SceneView.RepaintAll();
				}
			}
			
			if(moveIndex >= 0 && moveIndex != actions.Count-1)
			{
				BulletAction temp = actions[moveIndex];	
				actions[moveIndex] = actions[moveIndex+1];
				actions[moveIndex+1] = temp;
				
				bool temp2 = bp.btaFoldouts[i].sub[moveIndex];
				bp.btaFoldouts[i].sub[moveIndex] = bp.btaFoldouts[i].sub[moveIndex+1];
				bp.btaFoldouts[i].sub[moveIndex+1] = temp2;
			}
			
			if (removeIndex >= 0) 
			{
				actions.RemoveAt(removeIndex);
				bp.btaFoldouts[i].sub.RemoveAt(removeIndex);
			}
			
			EditorGUILayout.Space();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("");
			if (GUILayout.Button("Add Action", GUILayout.Width(100))) 
			{
				BulletAction ac = new BulletAction();
				actions.Add(ac);
				bp.btaFoldouts[i].sub.Add(true);
			}
			GUILayout.EndHorizontal();
			
			bp.bulletTags[i].actions = actions.ToArray();	
		}
	}	
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AttackPatternEditorWindow : EditorWindow
{
	private Vector2 tagScroll;
	private Vector2 actionScroll;
	private AttackPattern[] ap;
	private int apSelect;
	private bool fireOrBullet;
	private Dictionary<UnityEngine.Object, FoldoutTreeNode> roots;
	private int tagSelect;

	private class FoldoutTreeNode
	{
		public List<FoldoutTreeNode> children;
		public List<bool> foldouts;
		
		public FoldoutTreeNode()
		{
			children = new List<FoldoutTreeNode>();
			foldouts = new List<bool>();
		}
		
		public void Expand(bool recursive)
		{
			SetAll (true, recursive);
		}
		
		public void Collapse(bool recursive)
		{
			SetAll (false, recursive);
		}
		
		private void SetAll(bool value, bool recursive)
		{
			for(int i = 0; i < foldouts.Count; i++)
			{
				foldouts[i] = value;
				if(recursive && children[i] != null) 
				{
					children[i].SetAll(value, true);
				}
			}
		}
	}

	[MenuItem("Window/Attack Pattern Editor")]
	public static void ShowWindow()
	{
		AttackPatternEditorWindow bpew  = EditorWindow.GetWindow<AttackPatternEditorWindow> ("Attack Editor");
		bpew.tagScroll = new Vector2 (0f, 0f);
		bpew.actionScroll = new Vector2 (0f, 0f);
		bpew.ap = new AttackPattern[0];
		bpew.apSelect = -1;
		bpew.tagSelect = -1;
		bpew.fireOrBullet = false;
		bpew.roots = new Dictionary<UnityEngine.Object, FoldoutTreeNode> ();
	}

	void OnGUI()
	{
		if(ap == null)
		{
			ap = new AttackPattern[0];
		}
		EditorGUILayout.BeginHorizontal ();
		tagScroll = EditorGUILayout.BeginScrollView (tagScroll, GUILayout.MaxWidth(250));
		if(ap.Length > 1)
		{
			EditorGUILayout.LabelField("Attack Patterns");
			for(int i = 0; i < ap.Length; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button((i == apSelect) ? '\u2022'.ToString() : " " ,GUILayout.Width(20)))
				{
					apSelect = i;
					tagSelect = -1;
				}
				ap[i].bpName = EditorGUILayout.TextField(ap[i].bpName);
				EditorGUILayout.EndHorizontal();
			}
		}
		else
		{
			apSelect = 0;
		}
		if(ap.Length > 0 && apSelect >= 0)
		{
			ap[apSelect].fireTags = TagGUI<FireTag>("Fire Tags", false, ap[apSelect].fireTags);
			ap[apSelect].bulletTags = TagGUI<BulletTag>("Bullet Tag", true, ap[apSelect].bulletTags);
		}
		EditorGUILayout.EndScrollView ();
		actionScroll = EditorGUILayout.BeginScrollView (actionScroll);
		if(ap.Length > 0 && apSelect >= 0 && tagSelect >= 0)
		{
			UnityEngine.Object key;
			if(fireOrBullet)
			{
				key = ap[apSelect].bulletTags[tagSelect];
			}
			else
			{
				key = ap[apSelect].fireTags[tagSelect];
			}
			FoldoutTreeNode root;
			if(roots == null)
			{
				roots = new Dictionary<UnityEngine.Object, FoldoutTreeNode>();
			}
			if(roots.ContainsKey(key))
			{
				root = roots[key];
			}
			else
			{
				root = new FoldoutTreeNode();
				roots[key] = root;
			}
			if(fireOrBullet)
			{
				BulletActionsGUI((BulletTag)key, root);
			}
			else
			{
				FireActionsGUI((FireTag)key, root);
			}
		}
		EditorGUILayout.EndScrollView ();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		Global.Rank = (Rank)EditorGUILayout.EnumPopup(Global.Rank);
		GUILayout.Button ("Test");
		EditorGUILayout.EndHorizontal ();
		if(apSelect >= 0 && ap.Length > 0 && GUILayout.Button("Trim"))
		{
			for(int i = 0; i < ap[apSelect].fireTags.Length; i++)
			{
				FoldoutTreeNode parent = roots[ap[apSelect].fireTags[i]];
				for(int j = 0; j < ap[apSelect].fireTags[i].actions.Length; j++)
				{
					Trim (ap[apSelect].fireTags[i].actions[j], parent.children[j]);
				}
			}
			
			for(int i = 0; i < ap[apSelect].bulletTags.Length; i++)
			{
				FoldoutTreeNode parent = roots[ap[apSelect].bulletTags[i]];

				for(int j = 0; j < ap[apSelect].bulletTags[i].actions.Length; j++)
				{
					Trim (ap[apSelect].bulletTags[i].actions[j], parent.children[j]);
				}
			}
		}
	}

	private void Trim(FireAction action, FoldoutTreeNode foldout)
	{
		if(action.type != FireActionType.Repeat && action.nestedActions != null)
		{
			action.nestedActions = null;
			foldout.children = null;
		}
		else
		{
			for(int i = 0; i < action.nestedActions.Length; i++)
			{
				Trim (action.nestedActions[i], foldout.children[i]);
			}
		}
	}

	private void Trim(BulletAction action, FoldoutTreeNode foldout)
	{
		if(action.type != BulletActionType.Repeat && action.nestedActions != null)
		{
			action.nestedActions = null;
			foldout.children = null;
		}
		else
		{
			for(int i = 0; i < action.nestedActions.Length; i++)
			{
				Trim (action.nestedActions[i], foldout.children[i]);
			}
		}
	}

	void OnSelectionChange()
	{
		if(Selection.activeGameObject != null)
		{
			ap = Selection.activeGameObject.GetComponents<AttackPattern> ();
		}
		else
		{
			ap = new AttackPattern[0];
		}
		apSelect = -1;
		tagSelect = -1;
		Repaint ();
	}

	private T[] TagGUI<T>(string label, bool buttonEnable, T[] tags) where T : Tag, new()
	{
		EditorGUILayout.LabelField (label);
		if(tags == null || tags.Length < 1)
		{
			tags = new T[1];
			tags[0] = new T();
		}
		
		List<T> tagList = new List<T>(tags);

		bool buttonCheck = (fireOrBullet == buttonEnable);
		
		Vector3 moveRemove = new Vector3 (-1f, -1f, 0f);
		for(int i = 0; i < tagList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			if(GUILayout.Button((buttonCheck && (i == tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
			{
				fireOrBullet = buttonEnable;
				tagSelect = i;
			}
			tagList[i].tagName = EditorGUILayout.TextField (tagList[i].tagName);
			moveRemove = UpDownRemoveButtons(moveRemove, tagList.Count, i, true, buttonCheck);
			EditorGUILayout.EndHorizontal ();
		}
		MoveRemoveAdd<T>(moveRemove, tagList, null);
		return tagList.ToArray ();
	}

	private void BulletActionsGUI(BulletTag bulletTag, FoldoutTreeNode foldouts)
	{
		if (bulletTag.actions == null || bulletTag.actions.Length == 0)
		{
			bulletTag.actions = new BulletAction[1];
			bulletTag.actions[0] = new BulletAction();
			foldouts.children.Clear();
			foldouts.foldouts.Clear();
			foldouts.children.Add(null);
			foldouts.foldouts.Add(true);
		}
		
		EditorGUILayout.LabelField ("Bullet Tag: " + bulletTag.tagName);	
		GUILayout.BeginHorizontal();
		if(!bulletTag.randomSpeed)
			bulletTag.speed.x = EditorGUILayout.FloatField("Speed", bulletTag.speed.x);
		else
			bulletTag.speed = EditorGUILayout.Vector2Field("Speed Range", bulletTag.speed);
		bulletTag.randomSpeed = EditorGUILayout.Toggle("Randomize", bulletTag.randomSpeed);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		bulletTag.rankSpeed = EditorGUILayout.Toggle("Add Rank", bulletTag.rankSpeed);
		if(bulletTag.rankSpeed)
			bulletTag.speed.z = EditorGUILayout.FloatField("RankSpeed", bulletTag.speed.z);
		GUILayout.EndHorizontal();
		EditorGUILayout.ObjectField("Bullet Prefab ", bulletTag.prefab, typeof(GameObject), false);
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("Actions");
		if (GUILayout.Button("Expand All", GUILayout.Width(80)))
		{
			foldouts.Expand (true);
		}
		if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
		{
			foldouts.Collapse (true);
		}
		GUILayout.EndHorizontal();
		
		bulletTag.actions = BulletActionGUI(bulletTag.actions, foldouts);	
	}
	
	private void NestedBulletActionsGUI(BulletAction ba, FoldoutTreeNode foldouts)
	{
		if (ba.nestedActions == null || ba.nestedActions.Length == 0)
		{
			ba.nestedActions = new BulletAction[1];
			ba.nestedActions[0] = new BulletAction();
			foldouts.children.Clear();
			foldouts.foldouts.Clear();
			foldouts.children.Add(null);
			foldouts.foldouts.Add(true);
		}
		
		ba.nestedActions = BulletActionGUI(ba.nestedActions, foldouts);
	}

	private BulletAction[] BulletActionGUI(BulletAction[] bulletActions, FoldoutTreeNode foldouts)
	{
		List<BulletAction> actions = new List<BulletAction>(bulletActions);

		Vector3 moveRemove = new Vector3 (-1f, -1f, 0f);
		for (int i = 0; i < actions.Count; i++) 
		{
			GUILayout.BeginHorizontal();
			foldouts.foldouts[i] = EditorGUILayout.Foldout(foldouts.foldouts[i], "Action " + (i+1));
			moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i, false, false);
			GUILayout.EndHorizontal();
			if (foldouts.foldouts[i]) 
			{
				EditorGUI.indentLevel++;
				BulletAction ac = actions[i];
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
					
				case(BulletActionType.ChangeDirection):
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
					
				case(BulletActionType.ChangeSpeed):
				case(BulletActionType.VerticalChangeSpeed):
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
					
				case(BulletActionType.Repeat):
					ac.repeatCount.x = (int)EditorGUILayout.IntField("Repeat Count", (int)ac.repeatCount.x);
					GUILayout.BeginHorizontal();
					ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
					if(ac.rankRepeat)
						ac.repeatCount.y = EditorGUILayout.FloatField("RepeatRank", ac.repeatCount.y);
					GUILayout.EndHorizontal();
					if(foldouts.children[i] == null)
					{
						foldouts.children[i] = new FoldoutTreeNode();
					}
					NestedBulletActionsGUI(ac, foldouts.children[i]);
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
					ac.bulletTag = (BulletTag)EditorGUILayout.ObjectField(ac.bulletTag, typeof(BulletTag), false);
					break;
				}
				EditorGUI.indentLevel--;
			}
		}
		MoveRemoveAdd (moveRemove, actions, foldouts);
		return actions.ToArray();	
	}

	private void FireActionsGUI(FireTag fireTag, FoldoutTreeNode foldouts)
	{
		if (fireTag.actions == null || fireTag.actions.Length == 0)
		{
			fireTag.actions = new FireAction[1];
			fireTag.actions[0] = new FireAction();
			foldouts.children.Clear();
			foldouts.foldouts.Clear();
			foldouts.children.Add(null);
			foldouts.foldouts.Add(true);
		}
		
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("Fire Tag: " + fireTag.tagName);		
		if (GUILayout.Button("Expand All", GUILayout.Width(80)))
		{
			foldouts.Expand (true);
		}
		if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
		{
			foldouts.Collapse (true);
		}
		GUILayout.EndHorizontal();

		fireTag.actions = FireActionGUI(fireTag.actions, foldouts);	
	}

	private void NestedFireActionsGUI(FireAction fa, FoldoutTreeNode foldouts)
	{
		if (fa.nestedActions == null || fa.nestedActions.Length == 0)
		{
			fa.nestedActions = new FireAction[1];
			fa.nestedActions[0] = new FireAction();
			foldouts.children.Clear();
			foldouts.foldouts.Clear();
			foldouts.children.Add(null);
			foldouts.foldouts.Add(true);
		}
		
		fa.nestedActions = FireActionGUI(fa.nestedActions, foldouts);
	}
	
	private FireAction[] FireActionGUI(FireAction[] fireActions, FoldoutTreeNode foldouts)
	{
		List<FireAction> actions = new List<FireAction>(fireActions);
		
		Vector3 moveRemove = new Vector3 (-1f, -1f, 0f);
		for (int i = 0; i < actions.Count; i++) 
		{
			GUILayout.BeginHorizontal();
			foldouts.foldouts[i] = EditorGUILayout.Foldout(foldouts.foldouts[i], "Action " + (i+1));
			moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i, false, false);
			GUILayout.EndHorizontal();
			if(foldouts.foldouts[i])
			{
				EditorGUI.indentLevel++;
				FireAction ac = actions[i];
				
				ac.type = (FireActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);
				
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
					ac.bulletTag = (BulletTag)EditorGUILayout.ObjectField("Bullet Tag", ac.bulletTag, typeof(BulletTag), false);
					break;
					
				case(FireActionType.CallFireTag):
					ac.fireTag = (FireTag)EditorGUILayout.ObjectField("Fire Tag", ac.fireTag, typeof(FireTag), false);
					GUILayout.BeginHorizontal();
					ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
					if(!ac.passParam)
						ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
					GUILayout.EndHorizontal();	
					if(ac.passParam)
						ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
					break;
					
				case(FireActionType.Repeat):
					ac.repeatCount.x = (int)EditorGUILayout.IntField("RepeatCount", (int)ac.repeatCount.x);
					GUILayout.BeginHorizontal();
					ac.rankRepeat = EditorGUILayout.Toggle("AddRank", ac.rankRepeat);
					if(ac.rankRepeat)
						ac.repeatCount.y = EditorGUILayout.FloatField("RankRepeat", ac.repeatCount.y);
					GUILayout.EndHorizontal();
					if(foldouts.children[i] == null)
					{
						foldouts.children[i] = new FoldoutTreeNode();
					}
					NestedFireActionsGUI(ac, foldouts.children[i]);
					break;
				}
				EditorGUI.indentLevel--;
			}
		}
		MoveRemoveAdd<FireAction>(moveRemove, actions, foldouts);
		
		return actions.ToArray ();
	}

	private Vector3 UpDownRemoveButtons(Vector3 moveRemove, int count, int i, bool editTagSelect, bool buttonCheck)
	{
		GUI.enabled = (i > 0);
		if(GUILayout.Button('\u25B2'.ToString(), GUILayout.Width(22)))
		{
			moveRemove.x = i;		//Move Index
			moveRemove.z = -1f;		//Move Direction
			if(editTagSelect && buttonCheck && tagSelect == i)
			{
				tagSelect--;
			}
		}
		GUI.enabled = (i <  count - 1);
		if(GUILayout.Button('\u25BC'.ToString(), GUILayout.Width(22)))
		{
			moveRemove.x = i;		//Move Index
			moveRemove.z = 1f;		//Move Direction
			if(editTagSelect && buttonCheck && tagSelect == i)
			{
				tagSelect--;
			}
		}
		GUI.enabled = (count > 1);
		if(GUILayout.Button("X", GUILayout.Width(22)))
		{
			moveRemove.y = i;		//Remove Index
			if(editTagSelect && tagSelect == i)
			{
				tagSelect--;
			}
		}
		GUI.enabled = true;
		return moveRemove;
	}

	private static void MoveRemoveAdd<T>(Vector3 moveRemove, List<T> list, FoldoutTreeNode foldouts) where T : new()
	{
		if(moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			list.RemoveAt(removeIndex);
			if(foldouts !=  null)
			{
				foldouts.children.RemoveAt(removeIndex);
				foldouts.foldouts.RemoveAt(removeIndex);
			}
		}
		if(moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if(moveRemove.z > 0)
			{
				Swap<T>(list, moveIndex, moveIndex + 1);
				if(foldouts != null)
				{
					Swap<bool>(foldouts.foldouts, moveIndex, moveIndex + 1);
					Swap<FoldoutTreeNode>(foldouts.children, moveIndex, moveIndex + 1);
				}
			}
			if(moveRemove.z < 0)
			{
				Swap<T>(list, moveIndex, moveIndex - 1);
				if(foldouts != null)
				{
					Swap<bool>(foldouts.foldouts, moveIndex, moveIndex - 1);
					Swap<FoldoutTreeNode>(foldouts.children, moveIndex, moveIndex - 1);
				}
			}
		}
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Space (10 * EditorGUI.indentLevel);
		if(GUILayout.Button ("Add"))
		{
			list.Add (new T());
			if(foldouts != null)
			{
				foldouts.foldouts.Add(true);
				foldouts.children.Add(null);
			}
		}
		EditorGUILayout.EndHorizontal ();
	}

	private static void Swap<T>(List<T> list, int a, int b)
	{
		T temp = list[a];
		list[a] = list[b];
		list[b] = temp;
	}
}


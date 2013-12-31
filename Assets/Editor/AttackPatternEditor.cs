//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//
//[CustomEditor(typeof(AttackPattern))]
//public class AttackPatternEditor : Editor 
//{
//	private AttackPattern pattern;
//	public enum SelectionType { None, Movement, Fire, Bullet }
//
//	public FoldoutWrapper propertiesFoldout = new FoldoutWrapper();
//	public FoldoutWrapper dropsFoldout = new FoldoutWrapper();
//	public FoldoutWrapper movementFoldout = new FoldoutWrapper();
//	public FoldoutWrapper fireTagFoldout = new FoldoutWrapper();
//	public FoldoutWrapper bulletTagFoldout = new FoldoutWrapper();
//	public SelectionType selectType = SelectionType.None;
//	public int tagSelect;
//	
//	public class FoldoutWrapper
//	{
//		public bool Foldout;
//	}
//
//	public override void OnInspectorGUI()
//	{
//		pattern = target as AttackPattern;
//		propertiesFoldout.Foldout = EditorGUILayout.Foldout(propertiesFoldout.Foldout, "Properties");
//		if(propertiesFoldout.Foldout)
//		{
//			EditorGUI.indentLevel++;
//			pattern.Name = EditorGUILayout.TextField("Name", pattern.Name);
//			pattern.Title = EditorGUILayout.TextField("Title", pattern.Title);
//			pattern.health = EditorGUILayout.IntField("Health", pattern.health);
//			pattern.survival = EditorGUILayout.Toggle("Survival", pattern.survival);
//			pattern.bonus = EditorGUILayout.IntField("Bonus", pattern.bonus);
//			pattern.timeout = EditorGUILayout.IntField("Timeout", pattern.timeout);
//			EditorGUI.indentLevel--;
//		}
//		dropsFoldout.Foldout = EditorGUILayout.Foldout(dropsFoldout.Foldout, "Drops");
//		if(pattern.drops == null)
//		{
//			pattern.drops = new EnemyDrops();
//		}
//		if(dropsFoldout.Foldout)
//		{
//			EditorGUI.indentLevel++;
//			pattern.drops.power = EditorGUILayout.IntField("Power", pattern.drops.power);
//			pattern.drops.point = EditorGUILayout.IntField("Point", pattern.drops.point);
//			pattern.drops.life = EditorGUILayout.Toggle("Life", pattern.drops.life);
//			pattern.drops.bomb = EditorGUILayout.Toggle("Bomb", pattern.drops.bomb);
//			EditorGUI.indentLevel--;
//		}
//		if(GUI.changed)
//		{
//			EditorUtility.SetDirty(pattern);
//		}
//		TagGUI(pattern);
//		BottomControls();
//	}
//	
//	private void TagGUI(AttackPattern pattern)
//	{
//		movementFoldout.Foldout = EditorGUILayout.Foldout (movementFoldout.Foldout, "Movement Pattern");
//		if(movementFoldout.Foldout)
//		{
//			EditorGUILayout.BeginHorizontal();
//			if (GUILayout.Button((selectType == SelectionType.Movement) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
//			{
//				selectType = SelectionType.Movement;
//				tagSelect = -1;
//				Debug.Log(pattern);
//				TagEditorWindow.SetActionGroup<AttackPattern>(pattern, pattern);
//				RepaintImpl();
//			}
//			EditorGUILayout.LabelField("Movement Pattern");
//			EditorGUILayout.EndHorizontal();
//		}
//		pattern.fireTags = TagGUI<FireTag>(pattern, "Fire Tags", SelectionType.Fire, pattern.fireTags, fireTagFoldout);
//		pattern.bulletTags = TagGUI<BulletTag>(pattern, "Bullet Tags", SelectionType.Bullet, pattern.bulletTags, bulletTagFoldout);
//	}
//	
//	private void BottomControls()
//	{
//		EditorGUILayout.BeginHorizontal();
//		Rank testRank = (Rank)EditorGUILayout.EnumPopup(Global.Rank);
//		if(GUILayout.Button("Test"))
//		{
//			
//		}
//		EditorGUILayout.EndHorizontal();
//	}
//	
//	private T[] TagGUI<T>(AttackPattern pattern, string label, SelectionType buttonEnable, T[] tags, FoldoutWrapper fw) where T : Tag
//	{
//		fw.Foldout = EditorGUILayout.Foldout(fw.Foldout, label);
//		if (tags == null || tags.Length < 1)
//		{
//			tags = new T[1];
//			tags [0] = ScriptableObject.CreateInstance<T>();
//		}
//		
//		List<T> tagList = new List<T>(tags);
//		
//		if(fw.Foldout)
//		{
//			bool buttonCheck = (selectType == buttonEnable);
//			
//			Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
//			for (int i = 0; i < tagList.Count; i++)
//			{
//				if(tagList[tagSelect] == null)
//				{
//					tagList[i] = ScriptableObject.CreateInstance<T>();
//				}
//				EditorGUILayout.BeginHorizontal();
//				if (GUILayout.Button((buttonCheck && (i == tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
//				{
//					selectType = buttonEnable;
//					tagSelect = i;
//					TagEditorWindow.SetActionGroup<T>(tagList[tagSelect], pattern);
//					RepaintImpl();
//				}
//				tagList [i].Name = EditorGUILayout.TextField(tagList [i].Name);
//				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, tagList.Count, i, buttonCheck);
//				if(moveRemove.x > 0)
//				{
//					if (buttonCheck && tagSelect == i)
//					{
//						tagSelect += (int)moveRemove.z;
//					}
//					RepaintImpl();
//				}
//				if(moveRemove.y > 0)
//				{
//					Debug.Log("hello boo");
//					if (tagSelect == i)
//					{
//						tagSelect--;
//					}
//					RepaintImpl();
//				}
//				EditorGUILayout.EndHorizontal();
//			}
//			EditorUtils.MoveRemoveAddScriptableObject<T>(moveRemove, tagList);
//		}
//		
//		return tagList.ToArray();
//	}
//
//	
//	
//	private void RepaintImpl()
//	{
//		Repaint();
//		TagEditorWindow.instance.Repaint();
//	}
//}
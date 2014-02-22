using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DanmakuEngine.Core;

[CustomEditor(typeof(Boss))]
public class BossEditor : Editor 
{
	//	private Enemy enemy;
	private GameObject gameObject;
	private SerializedObject boss;
	
	public override void OnInspectorGUI ()
	{
		if(!DanmakuEditorUtils.DuplicateCheck<AbstractEnemy>(target as Boss))
		{
			return;
		}
		boss = serializedObject;
		gameObject = (target as Boss).gameObject;
		
		SerializedProperty name = boss.FindProperty ("bossName");
		SerializedProperty title = boss.FindProperty ("bossTitle");
		SerializedProperty theme = boss.FindProperty ("theme");
		SerializedProperty dropRadius = boss.FindProperty ("dropRadius");
		SerializedProperty start = boss.FindProperty ("startVector");
		SerializedProperty attackPatterns = boss.FindProperty ("attackPatterns");
		EditorGUILayout.PropertyField (name);
		EditorGUILayout.PropertyField (title);
		EditorGUILayout.PropertyField (theme);
		EditorGUILayout.PropertyField (dropRadius);
		EditorGUILayout.PropertyField (start, new GUIContent ("Start"));
		AttackPatternField (attackPatterns);
		boss.ApplyModifiedProperties ();
	}

	public void AttackPatternField(SerializedProperty attackPatterns)
	{
		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		int c = attackPatterns.FindPropertyRelative ("Array.size").intValue;
		if(c < 1)
		{
			attackPatterns.InsertArrayElementAtIndex(0);
			attackPatterns.GetArrayElementAtIndex(0).objectReferenceValue = gameObject.GetComponent<AbstractAttackPattern>();
		}
		for(int i = 0; i < c; i++)
		{
			EditorGUILayout.BeginHorizontal();
			SerializedProperty arrayElement = attackPatterns.GetArrayElementAtIndex(i);
			AbstractAttackPattern ap = (AbstractAttackPattern)arrayElement.objectReferenceValue;
			EditorGUILayout.PropertyField(arrayElement, new GUIContent((ap != null) ? GetAttackPatternName(ap) : i.ToString()));
			moveRemove = DanmakuEditorUtils.UpDownRemoveButtons(moveRemove, c, i, false);
			EditorGUILayout.EndHorizontal();
		}
		if (moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			if(attackPatterns.GetArrayElementAtIndex(removeIndex).objectReferenceValue != null)
			{
				attackPatterns.DeleteArrayElementAtIndex(removeIndex);
			}
			attackPatterns.DeleteArrayElementAtIndex(removeIndex);
		}
		if (moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if (moveRemove.z > 0)
			{
				attackPatterns.MoveArrayElement (moveIndex, moveIndex + 1);
			}
			if (moveRemove.z < 0)
			{
				attackPatterns.MoveArrayElement (moveIndex, moveIndex - 1);
			}
		}
		if (GUILayout.Button("Add"))
		{
			attackPatterns.InsertArrayElementAtIndex(c);
		}
	}

	private string GetAttackPatternName(AbstractAttackPattern ap)
	{
		string name;
		if(ap.Title != "")
		{
			name = ap.Title + " : " + ap.Name;
		}
		else
		{
			name = ap.Name;
		}
		return (name == "") ? "Attack Pattern" : name;
	}
	//	public void AttackPatternDetailGUI()
	//	{
	//		for(int i = 0; i < enemy.attackPatterns.Length; i++)
	//		{
	//			if(enemy.attackPatterns[i] == null)
	//			{
	//				enemy.attackPatterns[i] = new AttackPattern();
	//			}
	//			AttackPattern pattern = enemy.attackPatterns[i];
	//			if(enemy.boss)
	//			{
	//				pattern.main = EditorGUILayout.Foldout(pattern.main, GetAttackPatternName(pattern));
	//			}
	//			if(pattern.main || !enemy.boss)
	//			{
	//				EditorGUI.indentLevel++;
	//				if(enemy.boss)
	//				{
	//					pattern.propertiesFoldout.Foldout = EditorGUILayout.Foldout(pattern.propertiesFoldout.Foldout, "Properties");
	//					if(pattern.propertiesFoldout.Foldout)
	//					{
	//						EditorGUI.indentLevel++;
	//						pattern.Name = EditorGUILayout.TextField("Name", pattern.Name);
	//						pattern.Title = EditorGUILayout.TextField("Title", pattern.Title);
	//						pattern.health = EditorGUILayout.IntField("Health", pattern.health);
	//						pattern.survival = EditorGUILayout.Toggle("Survival", pattern.survival);
	//						pattern.bonus = EditorGUILayout.IntField("Bonus", pattern.bonus);
	//						pattern.timeout = EditorGUILayout.IntField("Timeout", pattern.timeout);
	//						EditorGUI.indentLevel--;
	//					}
	//				}
	//				pattern.dropsFoldout.Foldout = EditorGUILayout.Foldout(pattern.dropsFoldout.Foldout, "Drops");
	//				if(pattern.drops == null)
	//				{
	//					pattern.drops = new EnemyDrops();
	//				}
	//				if(pattern.dropsFoldout.Foldout)
	//				{
	//					EditorGUI.indentLevel++;
	//					pattern.drops.power = EditorGUILayout.IntField("Power", pattern.drops.power);
	//					pattern.drops.point = EditorGUILayout.IntField("Point", pattern.drops.point);
	//					pattern.drops.life = EditorGUILayout.Toggle("Life", pattern.drops.life);
	//					EditorGUI.indentLevel--;
	//				}
	//				TagGUI(pattern);
	//				BottomControls(i, pattern);
	//				EditorGUI.indentLevel--;
	//			}
	//		}
	//	}
	//	
	//	private void TagGUI(AttackPattern pattern)
	//	{
	//		pattern.movementFoldout.Foldout = EditorGUILayout.Foldout (pattern.movementFoldout.Foldout, "Movement Pattern");
	//		if(pattern.movementFoldout.Foldout)
	//		{
	//			EditorGUILayout.BeginHorizontal();
	//			if (GUILayout.Button((pattern.selectType == SelectionType.Movement) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
	//			{
	//				pattern.selectType = SelectionType.Movement;
	//				pattern.tagSelect = -1;
	//				Debug.Log(pattern);
	//				TagEditorWindow.SetActionGroup<AttackPattern>(pattern, enemy, pattern);
	//				RepaintImpl();
	//			}
	//			EditorGUILayout.LabelField("Movement Pattern");
	//			EditorGUILayout.EndHorizontal();
	//		}
	//		pattern.fireTags = TagGUI<FireTag>(pattern, "Fire Tags", SelectionType.Fire, pattern.fireTags, pattern.fireTagFoldout);
	//		pattern.bulletTags = TagGUI<BulletTag>(pattern, "Bullet Tags", SelectionType.Bullet, pattern.bulletTags, pattern.bulletTagFoldout);
	//	}
	//	
	//	private void BottomControls(int index, AttackPattern pattern)
	//	{
	//		EditorGUILayout.BeginHorizontal();
	//		pattern.testRank = (Rank)EditorGUILayout.EnumPopup(pattern.testRank);
	//		if(GUILayout.Button("Test") && !EditorApplication.isPlaying) 
	//		{
	//			GameObject temp = new GameObject();
	//			TestAttackPattern test = temp.AddComponent<TestAttackPattern>();
	//			test.enemyToAttachTo = enemy;
	//			test.patternToTest = index;
	//			test.testRank = pattern.testRank;
	//			temp.hideFlags = HideFlags.HideInHierarchy;
	//			EditorApplication.isPlaying = true;
	//		}
	//		EditorGUILayout.EndHorizontal();
	//	}
	//	
	//	private T[] TagGUI<T>(AttackPattern pattern, string label, SelectionType buttonEnable, T[] tags, FoldoutWrapper fw) where T : IActionGroup, NamedObject, new()
	//	{
	//		fw.Foldout = EditorGUILayout.Foldout(fw.Foldout, label);
	//		if (tags == null || tags.Length < 1)
	//		{
	//			tags = new T[1];
	//			tags [0] = new T();
	//		}
	//		
	//		List<T> tagList = new List<T>(tags);
	//		
	//		if(fw.Foldout)
	//		{
	//			bool buttonCheck = (pattern.selectType == buttonEnable);
	//			
	//			Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
	//			for (int i = 0; i < tagList.Count; i++)
	//			{
	//				if(tagList[i] == null)
	//				{
	//					tagList[i] = new T();
	//				}
	//				EditorGUILayout.BeginHorizontal();
	//				if (GUILayout.Button((buttonCheck && (i == pattern.tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
	//				{
	//					pattern.selectType = buttonEnable;
	//					pattern.tagSelect = i;
	//					TagEditorWindow.SetActionGroup<T>(tagList[pattern.tagSelect], enemy, pattern);
	//					RepaintImpl();
	//				}
	//				tagList [i].Name = EditorGUILayout.TextField(tagList [i].Name);
	//				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, tagList.Count, i, buttonCheck);
	//				if(moveRemove.x > 0)
	//				{
	//					if (buttonCheck && pattern.tagSelect == i)
	//					{
	//						pattern.tagSelect += (int)moveRemove.z;
	//					}
	//					RepaintImpl();
	//				}
	//				if(moveRemove.y > 0)
	//				{
	//					if (pattern.tagSelect == i)
	//					{
	//						pattern.tagSelect--;
	//					}
	//					RepaintImpl();
	//				}
	//				EditorGUILayout.EndHorizontal();
	//			}
	//			EditorUtils.MoveRemoveAdd<T>(moveRemove, tagList);
	//		}
	//		
	//		return tagList.ToArray();
	//	}
	//
	//
	//	private void RepaintImpl()
	//	{
	//		Repaint();
	//		TagEditorWindow.instance.Repaint();
	//	}
}
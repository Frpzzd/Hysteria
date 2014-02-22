using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DanmakuEngine.Core;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor 
{
//	private Enemy enemy;
//	private GameObject gameObject;
	private SerializedObject enemy;

	public override void OnInspectorGUI ()
	{
		if(!DanmakuEditorUtils.DuplicateCheck<AbstractEnemy>(target as Enemy))
		{
			return;
		}
		enemy = serializedObject;

		SerializedProperty name = enemy.FindProperty ("enemyName");
		SerializedProperty attackPattern = enemy.FindProperty ("attackPattern");
		EditorGUILayout.PropertyField (name);
		EditorGUILayout.PropertyField (attackPattern);
		enemy.ApplyModifiedProperties ();
	}

	void OnSceneGUI()
	{
		Enemy e = target as Enemy;
		Handles.DrawCamera (new Rect (0, 0, Screen.width, Screen.height), Camera.current);
		e.DrawHandles (e.Transform.position, false, Color.white);
	}

//	public override void OnInspectorGUI() 
//	{
//		enemy = target as Enemy;
//		gameObject = enemy.GameObject;
//		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
//		centeredStyle.alignment = TextAnchor.MiddleCenter;
//		Enemy[] scripts = gameObject.GetComponents<Enemy>();
//		if(scripts == null || scripts.Length < 1)
//		{
//			EditorGUILayout.LabelField("Wut... this shouldn't be happening", centeredStyle);
//		}
//		else if(scripts.Length > 1)
//		{
//			EditorGUILayout.LabelField("Object has too many Enemy scripts", centeredStyle);
//			if(GUILayout.Button("Keep Me (" + enemy.Name + ")"))
//			{
//				foreach(Enemy otherScript in gameObject.GetComponents<Enemy>())
//				{
//					if(otherScript != enemy)
//					{
//						DestroyImmediate(otherScript, true);
//					}
//				}
//				EditorUtility.SetDirty(gameObject);
//			}
//		}
//		else
//		{
//			enemy.Name = EditorGUILayout.TextField("Name", enemy.Name);
//			enemy.boss = EditorGUILayout.Toggle("Boss", enemy.boss);
//			if(enemy.boss)
//			{
//				EditorGUI.indentLevel++;
//				enemy.Title = EditorGUILayout.TextField("Title", enemy.Title);
//				enemy.bossTheme = (AudioClip)EditorGUILayout.ObjectField("Theme", enemy.bossTheme, typeof(AudioClip), false);
//				enemy.startYPosition = EditorGUILayout.FloatField("Start Y Position", enemy.startYPosition);
//				EditorGUI.indentLevel--;
//			}
//			else
//			{
//				enemy.Title = "";
//				if(enemy.attackPatterns == null || enemy.attackPatterns.Length < 1 || enemy.attackPatterns[0] == null)
//				{
//					enemy.attackPatterns = new AttackPattern[1];
//					enemy.attackPatterns[0] = new AttackPattern();
//				}
//				enemy.attackPatterns[0].health = EditorGUILayout.IntField("Health", enemy.attackPatterns[0].health);
//			}
//			enemy.dropRadius = EditorGUILayout.FloatField("Drop Radius", enemy.dropRadius);
//			AttackPatternGUI();
//			AttackPatternDetailGUI();
//		}
//		if(GUI.changed)
//		{
//			EditorUtility.SetDirty(enemy);
//		}
//	}
//
//	private void AttackPatternGUI()
//	{
//		if(enemy.boss)
//		{
//			EditorGUILayout.LabelField("Attack Pattern Execution Order");
//		}
//		if(enemy.attackPatterns == null || enemy.attackPatterns.Length < 1)
//		{
//			enemy.attackPatterns = new AttackPattern[1];
//		}
//
//		List<AttackPattern> apList = new List<AttackPattern>(enemy.attackPatterns);
//		
//		if(!enemy.boss && enemy.attackPatterns.Length > 1)
//		{
//			for(int i = 1; i < enemy.attackPatterns.Length; i++)
//			{
//				apList.RemoveAt(1);
//			}
//			Repaint();
//		}
//		
//		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
//		if(enemy.boss)
//		{
//			for (int i = 0; i < apList.Count; i++)
//			{
//				EditorGUILayout.BeginHorizontal();
//				EditorGUILayout.LabelField(GetAttackPatternName(apList[i]));
//				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, apList.Count, i, false);
//				EditorGUILayout.EndHorizontal();
//			}
//		}
//		if (moveRemove.y >= 0)
//		{
//			int removeIndex = (int)moveRemove.y;
//			apList.RemoveAt(removeIndex);
//		}
//		if (moveRemove.x >= 0)
//		{
//			int moveIndex = (int)moveRemove.x;
//			if (moveRemove.z > 0)
//			{
//				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex + 1);
//			}
//			if (moveRemove.z < 0)
//			{
//				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex - 1);
//			}
//		}
//		
//		EditorGUILayout.BeginHorizontal();
//		GUILayout.Space(10 * EditorGUI.indentLevel);
//		if(enemy.boss)
//		{
//			if (GUILayout.Button("Add"))
//			{
//				apList.Add(new AttackPattern());
//			}
//		}
//		EditorGUILayout.EndHorizontal();
//		enemy.attackPatterns = apList.ToArray();
//	}
//
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
//	private string GetAttackPatternName(AttackPattern ap)
//	{
//		if(ap.Title != "")
//		{
//			return ap.Title + " : " + ap.Name;
//		}
//		else
//		{
//			return ap.Name;
//		}
//	}
//
//	private void RepaintImpl()
//	{
//		Repaint();
//		TagEditorWindow.instance.Repaint();
//	}
}
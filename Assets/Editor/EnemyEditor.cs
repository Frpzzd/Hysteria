using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor 
{
	private Enemy enemy;
	private GameObject gameObject;
	private PatternFoldouts[] patternFoldouts;

	public override void OnInspectorGUI() 
	{
		enemy = target as Enemy;
		gameObject = enemy.GameObject;
		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.MiddleCenter;
		Enemy[] scripts = gameObject.GetComponents<Enemy>();
		if(scripts == null || scripts.Length < 1)
		{
			EditorGUILayout.LabelField("Wut... this shouldn't be happening", centeredStyle);
		}
		else if(scripts.Length > 1)
		{
			EditorGUILayout.LabelField("Object has too many Enemy scripts", centeredStyle);
			if(GUILayout.Button("Keep Me (" + enemy.Name + ")"))
			{
				foreach(Enemy otherScript in gameObject.GetComponents<Enemy>())
				{
					if(otherScript != enemy)
					{
						DestroyImmediate(otherScript, true);
					}
				}
				EditorUtility.SetDirty(gameObject);
			}
		}
		else
		{
			enemy.Name = EditorGUILayout.TextField("Name", enemy.Name);
			enemy.boss = EditorGUILayout.Toggle("Boss", enemy.boss);
			if(enemy.boss)
			{
				EditorGUI.indentLevel++;
				enemy.Title = EditorGUILayout.TextField("Title", enemy.Title);
				enemy.bossTheme = (AudioClip)EditorGUILayout.ObjectField("Theme", enemy.bossTheme, typeof(AudioClip), false);
				enemy.startYPosition = EditorGUILayout.FloatField("Start Y Position", enemy.startYPosition);
				EditorGUI.indentLevel--;
			}
			else
			{
				enemy.Title = "";
				enemy.attackPatterns[0].health = EditorGUILayout.IntField("Health", enemy.attackPatterns[0].health);
			}
			enemy.dropRadius = EditorGUILayout.FloatField("Drop Radius", enemy.dropRadius);
			AttackPatternGUI();
			AttackPatternDetailGUI();
		}
		if(GUI.changed)
		{
			EditorUtility.SetDirty(enemy);
		}
	}

	private void AttackPatternGUI()
	{
		if(enemy.boss)
		{
			EditorGUILayout.LabelField("Attack Pattern Execution Order");
		}
		if(enemy.attackPatterns == null || enemy.attackPatterns.Length < 1)
		{
			enemy.attackPatterns = new AttackPattern[1];
		}
		if(patternFoldouts == null)
		{
			patternFoldouts = new PatternFoldouts[enemy.attackPatterns.Length];
			for(int i = 0; i < patternFoldouts.Length; i++)
			{
				patternFoldouts[i] = new PatternFoldouts();
			}
		}

		List<AttackPattern> apList = new List<AttackPattern>(enemy.attackPatterns);
		List<PatternFoldouts> pfList = new List<PatternFoldouts> (patternFoldouts);
		
		if(!enemy.boss && enemy.attackPatterns.Length > 1)
		{
			for(int i = 1; i < enemy.attackPatterns.Length; i++)
			{
				apList.RemoveAt(1);
				pfList.RemoveAt(1);
			}
			Repaint();
		}
		
		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		if(enemy.boss)
		{
			for (int i = 0; i < apList.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(GetAttackPatternName(apList[i]));
				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, apList.Count, i, false);
				EditorGUILayout.EndHorizontal();
			}
		}
		if (moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			apList.RemoveAt(removeIndex);
		}
		if (moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if (moveRemove.z > 0)
			{
				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex + 1);
				EditorUtils.Swap<PatternFoldouts>(pfList, moveIndex, moveIndex + 1);
			}
			if (moveRemove.z < 0)
			{
				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex - 1);
				EditorUtils.Swap<PatternFoldouts>(pfList, moveIndex, moveIndex - 1);
			}
		}
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(10 * EditorGUI.indentLevel);
		if(enemy.boss)
		{
			if (GUILayout.Button("Add"))
			{
				apList.Add(new AttackPattern());
				pfList.Add(new PatternFoldouts());
			}
		}
		EditorGUILayout.EndHorizontal();
		enemy.attackPatterns = apList.ToArray();
		patternFoldouts = pfList.ToArray ();
	}
	public enum SelectionType { None, Movement, Fire, Bullet }

	public class PatternFoldouts
	{
		public bool main;
		public FoldoutWrapper propertiesFoldout = new FoldoutWrapper();
		public FoldoutWrapper dropsFoldout = new FoldoutWrapper();
		public FoldoutWrapper movementFoldout = new FoldoutWrapper();
		public FoldoutWrapper fireTagFoldout = new FoldoutWrapper();
		public FoldoutWrapper bulletTagFoldout = new FoldoutWrapper();
		public SelectionType selectType = SelectionType.None;
		public int tagSelect;
		public Rank testRank;
	}
	
	public class FoldoutWrapper
	{
		public bool Foldout;
	}

	public void AttackPatternDetailGUI()
	{
		for(int i = 0; i < enemy.attackPatterns.Length; i++)
		{
			if(enemy.attackPatterns[i] == null)
			{
				enemy.attackPatterns[i] = new AttackPattern();
			}
			if(patternFoldouts[i] == null)
			{
				patternFoldouts[i] = new PatternFoldouts();
			}
			AttackPattern pattern = enemy.attackPatterns[i];
			PatternFoldouts pf = patternFoldouts[i];
			if(enemy.boss)
			{
				pf.main = EditorGUILayout.Foldout(pf.main, GetAttackPatternName(pattern));
			}
			if(pf.main || !enemy.boss)
			{
				EditorGUI.indentLevel++;
				if(enemy.boss)
				{
					pf.propertiesFoldout.Foldout = EditorGUILayout.Foldout(pf.propertiesFoldout.Foldout, "Properties");
					if(pf.propertiesFoldout.Foldout)
					{
						EditorGUI.indentLevel++;
						pattern.Name = EditorGUILayout.TextField("Name", pattern.Name);
						pattern.Title = EditorGUILayout.TextField("Title", pattern.Title);
						pattern.health = EditorGUILayout.IntField("Health", pattern.health);
						pattern.survival = EditorGUILayout.Toggle("Survival", pattern.survival);
						pattern.bonus = EditorGUILayout.IntField("Bonus", pattern.bonus);
						pattern.timeout = EditorGUILayout.IntField("Timeout", pattern.timeout);
						EditorGUI.indentLevel--;
					}
				}
				pf.dropsFoldout.Foldout = EditorGUILayout.Foldout(pf.dropsFoldout.Foldout, "Drops");
				if(pattern.drops == null)
				{
					pattern.drops = new EnemyDrops();
				}
				if(pf.dropsFoldout.Foldout)
				{
					EditorGUI.indentLevel++;
					pattern.drops.power = EditorGUILayout.IntField("Power", pattern.drops.power);
					pattern.drops.point = EditorGUILayout.IntField("Point", pattern.drops.point);
					pattern.drops.life = EditorGUILayout.Toggle("Life", pattern.drops.life);
					pattern.drops.bomb = EditorGUILayout.Toggle("Bomb", pattern.drops.bomb);
					EditorGUI.indentLevel--;
				}
				TagGUI(pattern, pf);
				BottomControls(i, pf);
				EditorGUI.indentLevel--;
			}
		}
	}
	
	private void TagGUI(AttackPattern pattern, PatternFoldouts pf)
	{
		pf.movementFoldout.Foldout = EditorGUILayout.Foldout (pf.movementFoldout.Foldout, "Movement Pattern");
		if(pf.movementFoldout.Foldout)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button((pf.selectType == SelectionType.Movement) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
			{
				pf.selectType = SelectionType.Movement;
				pf.tagSelect = -1;
				Debug.Log(pattern);
				TagEditorWindow.SetActionGroup<AttackPattern>(pattern, enemy, pattern);
				RepaintImpl();
			}
			EditorGUILayout.LabelField("Movement Pattern");
			EditorGUILayout.EndHorizontal();
		}
		pattern.fireTags = TagGUI<FireTag>(pattern, "Fire Tags", SelectionType.Fire, pattern.fireTags, pf, pf.fireTagFoldout);
		pattern.bulletTags = TagGUI<BulletTag>(pattern, "Bullet Tags", SelectionType.Bullet, pattern.bulletTags, pf, pf.bulletTagFoldout);
	}
	
	private void BottomControls(int pattern, PatternFoldouts pf)
	{
		EditorGUILayout.BeginHorizontal();
		pf.testRank = (Rank)EditorGUILayout.EnumPopup(pf.testRank);
		if(GUILayout.Button("Test") && !EditorApplication.isPlaying) 
		{
			GameObject temp = new GameObject();
			TestAttackPattern test = temp.AddComponent<TestAttackPattern>();
			test.enemyToAttachTo = enemy;
			test.patternToTest = pattern;
			test.testRank = pf.testRank;
			temp.hideFlags = HideFlags.HideInHierarchy;
			EditorApplication.isPlaying = true;
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private T[] TagGUI<T>(AttackPattern pattern, string label, SelectionType buttonEnable, T[] tags, PatternFoldouts pf, FoldoutWrapper fw) where T : IActionGroup, NamedObject, new()
	{
		fw.Foldout = EditorGUILayout.Foldout(fw.Foldout, label);
		if (tags == null || tags.Length < 1)
		{
			tags = new T[1];
			tags [0] = new T();
		}
		
		List<T> tagList = new List<T>(tags);
		
		if(fw.Foldout)
		{
			bool buttonCheck = (pf.selectType == buttonEnable);
			
			Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
			for (int i = 0; i < tagList.Count; i++)
			{
				if(tagList[i] == null)
				{
					tagList[i] = new T();
				}
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button((buttonCheck && (i == pf.tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
				{
					pf.selectType = buttonEnable;
					pf.tagSelect = i;
					TagEditorWindow.SetActionGroup<T>(tagList[pf.tagSelect], enemy, pattern);
					RepaintImpl();
				}
				tagList [i].Name = EditorGUILayout.TextField(tagList [i].Name);
				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, tagList.Count, i, buttonCheck);
				if(moveRemove.x > 0)
				{
					if (buttonCheck && pf.tagSelect == i)
					{
						pf.tagSelect += (int)moveRemove.z;
					}
					RepaintImpl();
				}
				if(moveRemove.y > 0)
				{
					if (pf.tagSelect == i)
					{
						pf.tagSelect--;
					}
					RepaintImpl();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorUtils.MoveRemoveAdd<T>(moveRemove, tagList);
		}
		
		return tagList.ToArray();
	}

	private string GetAttackPatternName(AttackPattern ap)
	{
		if(ap.Title != "")
		{
			return ap.Title + " : " + ap.Name;
		}
		else
		{
			return ap.Name;
		}
	}

	
	
	private void RepaintImpl()
	{
		Repaint();
		TagEditorWindow.instance.Repaint();
	}
}
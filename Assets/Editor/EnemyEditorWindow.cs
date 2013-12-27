using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyEditorWindow : EditorWindow
{
	public static AttackPattern attackPattern;
    public static Enemy enemy
	{
		get
		{
			if(Selection.gameObjects != null && 
			   goSelect >= 0 &&
			   goSelect < Selection.gameObjects.Length &&
			   Selection.gameObjects[goSelect] != null &&
			   Selection.gameObjects[goSelect].GetComponents<Enemy>().Length == 1)
			{
				return Selection.gameObjects[goSelect].GetComponent<Enemy>();
			}
			else
			{
				return null;
			}
		}
	}

	private static EnemyEditorWindow _instance;
	public static EnemyEditorWindow instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<EnemyEditorWindow>("Enemy");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}

	private static int goSelect;
	private static int apSelect;

    void OnGUI()
    {
		if(Selection.gameObjects != null)
		{
			SelectionGUI();
			if(Selection.gameObjects.Length > 0 && goSelect >= 0 && goSelect < Selection.gameObjects.Length)
			{
				GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
				centeredStyle.alignment = TextAnchor.MiddleCenter;
				Enemy[] scripts = Selection.gameObjects[goSelect].GetComponents<Enemy>();
				if(scripts == null || scripts.Length < 1)
				{
					EditorGUILayout.LabelField("Object is not an Enemy", centeredStyle);
					if(GUILayout.Button("Make Enemy"))
					{
						Selection.gameObjects[goSelect].AddComponent<Enemy>();
						RepaintImpl ();
					}
				}
				else if(scripts.Length > 1)
				{
					EditorGUILayout.LabelField("Object has too many Enemy scripts", centeredStyle);
					int keepIndex = -1;
					for(int i = 0; i < scripts.Length; i++)
					{
						if(GUILayout.Button("Keep " + scripts[i].name))
						{
							keepIndex = i;
						}
					}
					if(keepIndex >= 0)
					{
						for(int i = 0; i < scripts.Length; i++)
						{
							if(i != keepIndex)
							{
								DestroyImmediate (scripts[i]);
							}
						}
						RepaintImpl ();
					}
				}
				else
				{
					enemy.Name = EditorGUILayout.TextField("Name", enemy.Name);
					enemy.boss = EditorGUILayout.Toggle("Boss", enemy.boss);
					if(enemy.boss)
					{
						EditorGUI.indentLevel++;
						enemy.title = EditorGUILayout.TextField("Title", enemy.title);
						EditorGUI.indentLevel--;
					}
					else
					{
						enemy.title = "";
						enemy.attackPatterns[0].health = EditorGUILayout.IntField("Health", enemy.attackPatterns[0].health);
					}
					enemy.dropRadius = EditorGUILayout.FloatField("Drop Radius", enemy.dropRadius);
					AttackPatternGUI();
				}
			}
		}
		else
		{
			RepaintImpl ();
		}
    }

	public void RepaintImpl ()
	{
		goSelect = -1;
		apSelect = -1;
		Repaint();
		AttackPatternTagEditorWindow.instance.Repaint();
		AttackPatternActionEditorWindow.instance.Repaint();
	}

	void OnSelectionChange()
	{
		RepaintImpl ();
	}

	void Update()
	{
		if(Selection.gameObjects != null)
		{
			bool changed = false;
			foreach(GameObject go in Selection.gameObjects)
			{
				foreach(Enemy enemy in go.GetComponents<Enemy>())
				{
					List<AttackPattern> patterns = new List<AttackPattern>(enemy.attackPatterns);
					List<int> toRemove = new List<int>();
					for(int i = 0; i < patterns.Count; i++)
					{
						if(patterns[i] == null)
						{
							toRemove.Add(i);
							changed = true;
						}
					}
					if(toRemove.Count > 0)
					{
						for(int i = 0; i < toRemove.Count; i++)
						{
							patterns.RemoveAt(i);
						}
						if(patterns.Count <= 0)
						{
							patterns.Add(go.AddComponent<AttackPattern>());
						}
						enemy.attackPatterns = patterns.ToArray();
					}
				}
			}
			if(changed)
			{
				RepaintImpl();
			}
		}
	}

    void SelectionGUI()
    {
		if(Selection.gameObjects.Length > 1)
		{
			goSelect = EditorUtils.UnityEngineObjectPopup(null, Selection.gameObjects, goSelect, "GameObject");
		}
		else
		{
			goSelect = 0;
		}
    }

    void AttackPatternGUI()
	{					
		EditorGUILayout.LabelField("Attack Patterns");
		if (enemy.attackPatterns == null || enemy.attackPatterns.Length < 1)
		{
			enemy.attackPatterns = new AttackPattern[1];
			enemy.attackPatterns[0] = enemy.gameObject.AddComponent<AttackPattern>();
		}

		List<AttackPattern> apList = new List<AttackPattern>(enemy.attackPatterns);

		if(!enemy.boss && enemy.attackPatterns.Length > 1)
		{
			for(int i = 1; i < enemy.attackPatterns.Length; i++)
			{
				DestroyImmediate(enemy.attackPatterns[i]);
				apList.RemoveAt(1);
			}
			apSelect = 0;
			attackPattern = enemy.attackPatterns[0];
			RepaintImpl();
		}

		Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
		for (int i = 0; i < apList.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button((i == apSelect) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
			{
				apSelect = i;
				Debug.Log(apSelect);
				attackPattern = enemy.attackPatterns[i];
			}
			apList [i].Name = EditorGUILayout.TextField(apList [i].Name);
			moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, apList.Count, i, false);
			if(moveRemove.x > 0)
			{
				if (apSelect == i)
				{
					apSelect += (int)moveRemove.z;
				}
				RepaintImpl ();
			}
			if(moveRemove.y > 0)
			{
				if (apSelect == i)
				{
					apSelect--;
				}
				RepaintImpl ();
			}
			EditorGUILayout.EndHorizontal();
		}
		if (moveRemove.y >= 0)
		{
			int removeIndex = (int)moveRemove.y;
			DestroyImmediate(apList[removeIndex]);
			apList.RemoveAt(removeIndex);
		}
		if (moveRemove.x >= 0)
		{
			int moveIndex = (int)moveRemove.x;
			if (moveRemove.z > 0)
			{
				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex + 1);
			}
			if (moveRemove.z < 0)
			{
				EditorUtils.Swap<AttackPattern>(apList, moveIndex, moveIndex - 1);
			}
		}

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(10 * EditorGUI.indentLevel);
		if(enemy.boss)
		{
			if (GUILayout.Button("Add"))
			{
				apList.Add(enemy.gameObject.AddComponent<AttackPattern>());
			}
		}
		EditorGUILayout.EndHorizontal();
		enemy.attackPatterns = apList.ToArray();
    }

    private bool CheckEnemy()
    {
        return false;
    }
}


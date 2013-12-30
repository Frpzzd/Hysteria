using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AttackPatternTagEditorWindow : EditorWindow
{
	private static AttackPatternTagEditorWindow _instance;
	public static AttackPatternTagEditorWindow instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = EditorWindow.GetWindow<AttackPatternTagEditorWindow>("Attack Pattern");
			}
			return _instance;
		}

		set
		{
			_instance = value;
		}
	}
    private Vector2 scroll;
	public static AttackPattern attackPattern { get { return EnemyEditorWindow.attackPattern; } }
	public static Enemy enemy { get { return EnemyEditorWindow.enemy; } }
    private static SelectionType selectType = SelectionType.None;
    public static int tagSelect;
	public bool windowChanged = false;
	private enum SelectionType { None, Movement, Fire, Bullet }

    [MenuItem("Window/Enemy Editor")]
    public static void ShowWindow()
    {
		EnemyEditorWindow.instance = EditorWindow.GetWindow<EnemyEditorWindow> ("Enemy");
		instance = EditorWindow.GetWindow<AttackPatternTagEditorWindow>("Attack Pattern");
		ActionGroupEditorWindow.instance = EditorWindow.GetWindow<ActionGroupEditorWindow>("Actions");
    }

    void OnGUI()
    {
		instance = this;
		scroll = EditorGUILayout.BeginScrollView(scroll);
		if(enemy != null && attackPattern != null)
		{
			attackPattern.parent = enemy;
			if(enemy.boss)
			{
				attackPattern.bpName = EditorGUILayout.TextField("Name", attackPattern.bpName);
				attackPattern.health = EditorGUILayout.IntField("Health", attackPattern.health);
				attackPattern.survival = EditorGUILayout.Toggle("Survival", attackPattern.survival);
				attackPattern.bonus = EditorGUILayout.IntField("Bonus", attackPattern.bonus);
				attackPattern.timeout = EditorGUILayout.IntField("Timeout", attackPattern.timeout);
			}
			EditorGUILayout.LabelField("Drops");
			EditorGUI.indentLevel++;
			attackPattern.drops.power = EditorGUILayout.IntField("Power", attackPattern.drops.power);
			attackPattern.drops.point = EditorGUILayout.IntField("Point", attackPattern.drops.point);
			attackPattern.drops.life = EditorGUILayout.Toggle("Life", attackPattern.drops.life);
			attackPattern.drops.bomb = EditorGUILayout.Toggle("Bomb", attackPattern.drops.bomb);
			EditorGUI.indentLevel--;
			if(GUI.changed)
			{
				EditorUtility.SetDirty(enemy);
				EditorUtility.SetDirty(attackPattern);
			}
		}
		TagGUI();
		EditorGUILayout.EndScrollView();
        BottomControls();
    }

	void Update()
	{
		if(windowChanged)
		{
			Repaint();
			ActionGroupEditorWindow.instance.Repaint();
		}
	}

    private void TagGUI()
    {
        if (attackPattern != null)
        {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button((selectType == SelectionType.Movement) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
			{
				selectType = SelectionType.Movement;
				tagSelect = -1;
				ChangeActionGroup(attackPattern);
				windowChanged = true;
			}
			EditorGUILayout.LabelField("Movement Pattern");
			EditorGUILayout.EndHorizontal();
            attackPattern.fireTags = TagGUI<FireTag>("Fire Tags", SelectionType.Fire, attackPattern.fireTags);
            attackPattern.bulletTags = TagGUI<BulletTag>("Bullet Tags", SelectionType.Bullet, attackPattern.bulletTags);
        }
    }

    void BottomControls()
    {
        EditorGUILayout.BeginHorizontal();
        Global.Rank = (Rank)EditorGUILayout.EnumPopup(Global.Rank);
        GUILayout.Button("Test");
        EditorGUILayout.EndHorizontal();
    }

    private T[] TagGUI<T>(string label, SelectionType buttonEnable, T[] tags) where T : NamedObject, new()
    {
		if(attackPattern != null)
		{
			EditorGUILayout.LabelField(label);
			if (tags == null || tags.Length < 1)
			{
				tags = new T[1];
				tags [0] = new T();
			}
			
			List<T> tagList = new List<T>(tags);
			
			bool buttonCheck = (selectType == buttonEnable);
			
			Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
			for (int i = 0; i < tagList.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button((buttonCheck && (i == tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
				{
					selectType = buttonEnable;
					tagSelect = i;
					switch(selectType)
					{
						case SelectionType.Fire:
							ChangeActionGroup(attackPattern.fireTags[tagSelect]);
							break;
						case SelectionType.Bullet:
							ChangeActionGroup(attackPattern.bulletTags[tagSelect]);
							break;
					}
					windowChanged = true;
				}
				tagList [i].Name = EditorGUILayout.TextField(tagList [i].Name);
				moveRemove = EditorUtils.UpDownRemoveButtons(moveRemove, tagList.Count, i, buttonCheck);
				if(moveRemove.x > 0)
				{
					if (buttonCheck && tagSelect == i)
					{
						tagSelect += (int)moveRemove.z;
					}
					windowChanged = true;
				}
				if(moveRemove.y > 0)
				{
					if (tagSelect == i)
					{
						tagSelect--;
					}
					windowChanged = true;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorUtils.MoveRemoveAdd<T>(moveRemove, tagList);
			return tagList.ToArray();
		}
		else
		{
			return tags;
		}
    }

	private static void ChangeActionGroup(IActionGroup group)
	{
		ActionGroupEditorWindow.editing = true;
		ActionGroupEditorWindow.parameters = new object[]{ attackPattern };
		ActionGroupEditorWindow.actionGroup = group;
		ActionGroupEditorWindow.editing = false;
		ActionGroupEditorWindow.instance.Repaint();
	}
}
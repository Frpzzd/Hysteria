using System;
using UnityEditor;
using UnityEngine;

public class EnemyEditorWindow : EditorWindow
{
    private Enemy enemy = null;
    private Enemy[] excessEnemyScripts = new Enemy[0];
    private int apSelect = -1;

    [MenuItem("Window/Enemy Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<EnemyEditorWindow>("Attack Pattern Editor");
    }

    void OnGUI()
    {
        if (CheckEnemy())
        {
            EditorGUILayout.BeginHorizontal();
            SelectGUI();
            AttackPatternGUI();
            EditorGUILayout.EndHorizontal();
        }
    }

    void SelectGUI()
    {
        throw new NotImplementedException();
    }

    void AttackPatternGUI()
    {
        throw new NotImplementedException();
    }

    private bool CheckEnemy()
    {
        if (enemy == null)
        {
            EditorGUILayout.LabelField("Selected GameObject is not an Enemy");
            if (GUILayout.Button("Make Enemy"))
            {
                enemy = Selection.activeGameObject.AddComponent<Enemy>();
                Repaint();
            }
        }
        else
        {
            if (excessEnemyScripts.Length > 1)
            {
                EditorGUILayout.LabelField("Too Many Enemy Scripts on GameObject, remove until one remains");
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            enemy =  Selection.activeGameObject.GetComponent<Enemy>();
            excessEnemyScripts = Selection.activeGameObject.GetComponents<Enemy>();
        } 
        else
        {
            enemy = null;
            excessEnemyScripts = new Enemy[0];
        }
        apSelect = -1;
        Repaint();
    }
}


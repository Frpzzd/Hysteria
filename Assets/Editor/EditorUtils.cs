using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorUtils
{
    public static void ExpandCollapseButtons(string label, FoldoutTreeNode foldouts)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        if (GUILayout.Button("Expand All", GUILayout.Width(80)))
        {
            foldouts.Expand(true);
        }
        if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
        {
            foldouts.Collapse(true);
        }
        EditorGUILayout.EndHorizontal();
    }

    public static int NamedObjectPopup(string label, NamedObject[] objects, int selectedIndex, string nullName)
    {
        Dictionary<string, int> repeats = new Dictionary<string, int>();
        List<string> names = new List<string>(objects.Length);
        for (int i = 0; i < objects.Length; i++)
        {
            if (names.Contains(objects [i].Name))
            {
                if (objects [i].Name == null)
                {
                    objects [i].Name = nullName;
                }
                if (repeats.ContainsKey(objects [i].Name))
                {
                    repeats [objects [i].Name]++;
                } else
                {
                    repeats [objects [i].Name] = 1;
                }
                names.Add(objects [i].Name + " " + (repeats [objects [i].Name] + 1));
            } else
            {
                names.Add(objects [i].Name);
            }
        }
        selectedIndex = EditorGUILayout.Popup(label, selectedIndex, names.ToArray());
        if (selectedIndex < 0 || selectedIndex >= objects.Length)
        {
            selectedIndex = -1;
        }
        return selectedIndex;
    }

    public static void MoveRemoveAdd<T>(Vector3 moveRemove, List<T> list, FoldoutTreeNode foldouts) where T : new()
    {
        if (moveRemove.y >= 0)
        {
            int removeIndex = (int)moveRemove.y;
            list.RemoveAt(removeIndex);
            if (foldouts != null)
            {
                foldouts.children.RemoveAt(removeIndex);
                foldouts.foldouts.RemoveAt(removeIndex);
            }
        }
        if (moveRemove.x >= 0)
        {
            int moveIndex = (int)moveRemove.x;
            if (moveRemove.z > 0)
            {
                Swap<T>(list, moveIndex, moveIndex + 1);
                if (foldouts != null)
                {
                    Swap<bool>(foldouts.foldouts, moveIndex, moveIndex + 1);
                    Swap<FoldoutTreeNode>(foldouts.children, moveIndex, moveIndex + 1);
                }
            }
            if (moveRemove.z < 0)
            {
                Swap<T>(list, moveIndex, moveIndex - 1);
                if (foldouts != null)
                {
                    Swap<bool>(foldouts.foldouts, moveIndex, moveIndex - 1);
                    Swap<FoldoutTreeNode>(foldouts.children, moveIndex, moveIndex - 1);
                }
            }
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10 * EditorGUI.indentLevel);
        if (GUILayout.Button("Add"))
        {
            list.Add(new T());
            if (foldouts != null)
            {
                foldouts.foldouts.Add(true);
                foldouts.children.Add(null);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private static void Swap<T>(List<T> list, int a, int b)
    {
        T temp = list [a];
        list [a] = list [b];
        list [b] = temp;
    }
}

public class FoldoutTreeNode
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
        SetAll(true, recursive);
    }
    
    public void Collapse(bool recursive)
    {
        SetAll(false, recursive);
    }
    
    private void SetAll(bool value, bool recursive)
    {
        for (int i = 0; i < foldouts.Count; i++)
        {
            foldouts [i] = value;
            if (recursive && children [i] != null)
            {
                children [i].SetAll(value, true);
            }
        }
    }
}


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AttackPatternTagEditorWindow : EditorWindow
{
    private Vector2 scroll;
    private AttackPatternActionEditorWindow apaew;
    public AttackPattern[] ap;
    public bool fireOrBullet;
    public int apSelect;
    public int tagSelect;

    public AttackPattern currentAp
    {
        get
        {
            if(ap != null && ap.Length > 0 && apSelect >= 0 && apSelect < ap.Length)
            {
                return ap[apSelect];
            }
            else
            {
                return null;
            }
        }
    }

    public NamedObject currentTag
    {
        get
        {
            NamedObject tag = null;
            if(ap != null && ap.Length > 0 && apSelect >= 0 && apSelect < ap.Length && tagSelect >= 0)
            {
                if(fireOrBullet)
                {
                    if(ap[apSelect].bulletTags.Length > tagSelect)
                    {
                        tag = ap[apSelect].bulletTags[tagSelect];
                    }
                }
                else
                {
                    if(ap[apSelect].fireTags.Length > tagSelect)
                    {
                        tag = ap[apSelect].fireTags[tagSelect];
                    }
                }
            }
            return tag;
        }
    }

    [MenuItem("Window/Attack Pattern Editor")]
    public static void ShowWindow()
    {
        AttackPatternTagEditorWindow aptew = EditorWindow.GetWindow<AttackPatternTagEditorWindow>("Tags");
        aptew.apaew = EditorWindow.GetWindow<AttackPatternActionEditorWindow>("Actions");
        aptew.apaew.aptew = aptew;
    }

    void OnGUI()
    {
        APSelect();
        TagGUI();
        BottomControls();
    }

    void Update()
    {
        if (Selection.activeGameObject != null && ap != null)
        {
            bool changed = false;
            AttackPattern[] patterns = Selection.activeGameObject.GetComponents<AttackPattern>();
            if(patterns.Length == ap.Length)
            {
                for(int i = 0; i < ap.Length; i++)
                {
                    if(patterns[i] != ap[i])
                    {
                        changed = true;
                    }
                }
            }
            else
            {
                changed = true;
            }
            if(changed)
            {
                ap = patterns;
                Repaint();
            }
        }
    }

    void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            ap = Selection.activeGameObject.GetComponents<AttackPattern>();
        } 
        else
        {
            ap = new AttackPattern[0];
        }
        apSelect = -1;
        tagSelect = -1;
        Repaint();
    }

    private void APSelect()
    {
        if (ap == null)
        {
            ap = new AttackPattern[0];
        }
        if (ap.Length > 1)
        {
            int oldSelect = apSelect;
            apSelect = EditorUtils.NamedObjectPopup(null, ap, apSelect, "Attack Pattern");
            if(apSelect != oldSelect)
            {
                tagSelect = -1;
            }
        } 
        else
        {
            apSelect = 0;
        }
    }

    private void TagGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (ap.Length > 0 && apSelect >= 0)
        {
            ap [apSelect].fireTags = TagGUI<FireTag>("Fire Tags", false, ap[apSelect].fireTags);
            ap [apSelect].bulletTags = TagGUI<BulletTag>("Bullet Tags", true, ap[apSelect].bulletTags);
        }
        EditorGUILayout.EndScrollView();
    }

    void BottomControls()
    {
        EditorGUILayout.BeginHorizontal();
        Global.Rank = (Rank)EditorGUILayout.EnumPopup(Global.Rank);
        GUILayout.Button("Test");
        EditorGUILayout.EndHorizontal();
    }

    private T[] TagGUI<T>(string label, bool buttonEnable, T[] tags) where T : NamedObject, new()
    {
        EditorGUILayout.LabelField(label);
        if (tags == null || tags.Length < 1)
        {
            tags = new T[1];
            tags [0] = new T();
        }
        
        List<T> tagList = new List<T>(tags);
        
        Debug.Log(tags[0].Name);
        bool buttonCheck = (fireOrBullet == buttonEnable);

        Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
        for (int i = 0; i < tagList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button((buttonCheck && (i == tagSelect)) ? '\u2022'.ToString() : " ", GUILayout.Width(20)))
            {
                fireOrBullet = buttonEnable;
                tagSelect = i;
                apaew.Repaint();
            }
            tagList [i].Name = EditorGUILayout.TextField(tagList [i].Name);
            moveRemove = UpDownRemoveButtons(moveRemove, tagList.Count, i, buttonCheck);
            EditorGUILayout.EndHorizontal();
        }
        EditorUtils.MoveRemoveAdd<T>(moveRemove, tagList, null);
        return tagList.ToArray();
    }

    public Vector3 UpDownRemoveButtons(Vector3 moveRemove,int count, int i, bool buttonCheck)
    {
        GUI.enabled = (i > 0);
        if (GUILayout.Button('\u25B2'.ToString(), GUILayout.Width(22)))
        {
            moveRemove.x = i;       //Move Index
            moveRemove.z = -1f;     //Move Direction
            if (buttonCheck && tagSelect == i)
            {
                tagSelect--;
            }
        }
        GUI.enabled = (i < count - 1);
        if (GUILayout.Button('\u25BC'.ToString(), GUILayout.Width(22)))
        {
            moveRemove.x = i;       //Move Index
            moveRemove.z = 1f;      //Move Direction
            if (buttonCheck && tagSelect == i)
            {
                tagSelect++;
            }
        }
        GUI.enabled = (count > 1);
        if (GUILayout.Button("X", GUILayout.Width(22)))
        {
            moveRemove.y = i;       //Remove Index
            if (tagSelect == i)
            {
                tagSelect--;
            }
        }
        GUI.enabled = true;
        return moveRemove;
    }
}


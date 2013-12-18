using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackPatternActionEditorWindow : EditorWindow
{
    private Vector2 actionScroll;
    public AttackPatternTagEditorWindow aptew;
    public Dictionary<BulletTag, FoldoutTreeNode> bulletRoots;
    public Dictionary<FireTag, FoldoutTreeNode> fireRoots;
    private NamedObject currentTag;

    private NamedObject tag
    {
        get { return aptew.currentTag; }
    }

    void OnGUI()
    {
        actionScroll = EditorGUILayout.BeginScrollView(actionScroll);
        if (tag != null)
        {
            if (tag is BulletTag)
            {
                BulletTag bulletTag = tag as BulletTag;
                if(bulletRoots == null)
                {
                    bulletRoots = new Dictionary<BulletTag, FoldoutTreeNode>();
                }
                if(!bulletRoots.ContainsKey(bulletTag))
                {
                    bulletRoots[bulletTag] = new FoldoutTreeNode();
                }
                BulletActionsGUI(bulletTag, bulletRoots[bulletTag]);
            } 
            else if(tag is FireTag)
            {
                FireTag fireTag = tag as FireTag;
                if(fireRoots == null)
                {
                    fireRoots = new Dictionary<FireTag, FoldoutTreeNode>();
                }
                if(!fireRoots.ContainsKey(fireTag))
                {
                    fireRoots[fireTag] = new FoldoutTreeNode();
                }
                FireActionsGUI(fireTag, fireRoots[fireTag]);
            }
        }
        EditorGUILayout.EndScrollView();
        if (tag != null)
        {
            if (GUILayout.Button("Trim"))
            {
//                if(tag is BulletTag)
//                {
//                    for (int j = 0; j < ((BulletTag)tag).actions.Length; j++)
//                    {
//                        Trim(((BulletTag)tag).actions[j]);
//                    }
//                }
//                else if(tag is FireTag)
//                {
//                    for (int j = 0; j < ((FireTag)tag).actions.Length; j++)
//                    {
//                        Trim(((FireTag)tag).actions[j]);
//                    }
//                }
            }
        }
    }

    void Update()
    {
        if (currentTag != tag)
        {
            currentTag = tag;
            Repaint();
        }
    }

    private void Trim(FireAction action)
    {
        if (action.type != FireActionType.Repeat && action.nestedActions != null)
        {
            action.nestedActions = null;
        } 
        else
        {
            for (int i = 0; i < action.nestedActions.Length; i++)
            {
                Trim(action.nestedActions [i]);
            }
        }
    }
    
    private void Trim(BulletAction action)
    {
        if (action.type != BulletActionType.Repeat && action.nestedActions != null)
        {
            action.nestedActions = null;
        } 
        else
        {
            for (int i = 0; i < action.nestedActions.Length; i++)
            {
                Trim(action.nestedActions [i]);
            }
        }
    }

    private void BulletActionsGUI(BulletTag bulletTag, FoldoutTreeNode foldouts)
    {
        if (bulletTag.actions == null || bulletTag.actions.Length == 0)
        {
            bulletTag.actions = new BulletAction[0];
            ResetFoldouts(foldouts, true);
        }
        
        EditorGUILayout.LabelField("Bullet Tag: " + bulletTag.Name);
        bulletTag.speed = AttackPatternPropertyField("Speed", bulletTag.speed, false);
        
        EditorUtils.ExpandCollapseButtons("Actions", foldouts);
        
        bulletTag.actions = BulletActionGUI(bulletTag.actions, foldouts);   
    }
    
    private void NestedBulletActionsGUI(BulletAction ba, FoldoutTreeNode foldouts)
    {
        if (ba.nestedActions == null || ba.nestedActions.Length == 0)
        {
            ba.nestedActions = new BulletAction[1];
            ba.nestedActions [0] = new BulletAction();
            ResetFoldouts(foldouts);
        }
        
        ba.nestedActions = BulletActionGUI(ba.nestedActions, foldouts);
    }
    
    private void FireActionsGUI(FireTag fireTag, FoldoutTreeNode foldouts)
    {
        if (fireTag.actions == null || fireTag.actions.Length == 0)
        {
            fireTag.actions = new FireAction[1];
            fireTag.actions [0] = new FireAction();
            ResetFoldouts(foldouts);
        }

        EditorUtils.ExpandCollapseButtons("Fire Tag: " + fireTag.Name, foldouts);
        
        fireTag.actions = FireActionGUI(fireTag.actions, foldouts); 
    }
    
    private void NestedFireActionsGUI(FireAction fa, FoldoutTreeNode foldouts)
    {
        if (fa.nestedActions == null || fa.nestedActions.Length == 0)
        {
            fa.nestedActions = new FireAction[1];
            fa.nestedActions [0] = new FireAction();
            ResetFoldouts(foldouts);
        }
        
        fa.nestedActions = FireActionGUI(fa.nestedActions, foldouts);
    }
    
    private BulletAction[] BulletActionGUI(BulletAction[] bulletActions, FoldoutTreeNode foldouts)
    {
        List<BulletAction> actions = new List<BulletAction>(bulletActions);
        
        Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
        for (int i = 0; i < actions.Count; i++)
        {
            Rect boundingRect = EditorGUILayout.BeginVertical();
            GUI.Box(boundingRect, "");
            EditorGUILayout.BeginHorizontal();
            foldouts.foldouts [i] = EditorGUILayout.Foldout(foldouts.foldouts [i], "Action " + (i + 1));
            moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i, true);
            EditorGUILayout.EndHorizontal();
            if (foldouts.foldouts [i])
            {
                EditorGUI.indentLevel++;
                BulletAction ac = actions [i];
                ac.type = (BulletActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);
                
                switch (ac.type)
                {
                    case(BulletActionType.Wait):
                        ac.wait = AttackPatternPropertyField("Wait", ac.wait, false);
                        break;
                        
                    case(BulletActionType.ChangeDirection):
                        ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                        ac.angle = AttackPatternPropertyField("Angle", ac.angle, false);
                        ac.wait = AttackPatternPropertyField("Time", ac.wait, false);
                        ac.waitForChange = EditorGUILayout.Toggle("Wait To Finish", ac.waitForChange);
                        break;
                        
                    case(BulletActionType.ChangeSpeed):
                    case(BulletActionType.VerticalChangeSpeed):
                        ac.speed = AttackPatternPropertyField("Speed", ac.speed, false);
                        ac.wait = AttackPatternPropertyField("Time", ac.wait, false);
                        ac.waitForChange = EditorGUILayout.Toggle("Wait To Finish", ac.waitForChange);
                        break;
                        
                    case(BulletActionType.Repeat):
                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
                        if (foldouts.children [i] == null)
                        {
                            foldouts.children [i] = new FoldoutTreeNode();
                        }
                        NestedBulletActionsGUI(ac, foldouts.children [i]);
                        break;
                        
                    case(BulletActionType.Fire):    
                        ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                        
                        if (!ac.useParam)
                        {
                            ac.angle = AttackPatternPropertyField("Angle", ac.angle, false);
                        }
                        ac.useParam = EditorGUILayout.Toggle("Use Param Angle", ac.useParam);
                        EditorGUILayout.Space();
                        ac.overwriteBulletSpeed = EditorGUILayout.Toggle("OverwriteSpeed", ac.overwriteBulletSpeed);
                        if (ac.overwriteBulletSpeed)
                        {
                            ac.speed = AttackPatternPropertyField("Speed", ac.speed, false);
                        }
                        EditorGUILayout.Space();
                        ac.bulletTagIndex = EditorUtils.NamedObjectPopup("Bullet Tag", aptew.currentAp.bulletTags, ac.bulletTagIndex, "Bullet Tag");
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        EditorUtils.MoveRemoveAdd(moveRemove, actions, foldouts);
        return actions.ToArray();   
    }
    
    private FireAction[] FireActionGUI(FireAction[] fireActions, FoldoutTreeNode foldouts)
    {
        List<FireAction> actions = new List<FireAction>(fireActions);
        
        Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
        for (int i = 0; i < actions.Count; i++)
        {
            Rect boundingRect = EditorGUILayout.BeginVertical();
            GUI.Box(boundingRect, "");
            EditorGUILayout.BeginHorizontal();
            foldouts.foldouts [i] = EditorGUILayout.Foldout(foldouts.foldouts [i], "Action " + (i + 1));
            moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i);
            EditorGUILayout.EndHorizontal();
            if (foldouts.foldouts [i])
            {
                EditorGUI.indentLevel++;    
                FireAction ac = actions [i];
                
                ac.type = (FireActionType)EditorGUILayout.EnumPopup("Action Type", ac.type);
                
                switch (ac.type)
                {
                    case(FireActionType.Wait):
                        ac.wait = AttackPatternPropertyField("Wait", ac.wait, false);
                        break;
                        
                    case(FireActionType.Fire):
                        ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                        if (!ac.useParam)
                        {
                            ac.angle = AttackPatternPropertyField("Angle", ac.angle, false);
                        }
                        ac.useParam = EditorGUILayout.Toggle("Use Param Angle", ac.useParam);
                        EditorGUILayout.Space();
                        
                        EditorGUILayout.BeginHorizontal();
                        ac.overwriteBulletSpeed = EditorGUILayout.Toggle("Overwrite Speed", ac.overwriteBulletSpeed);
                        if(ac.overwriteBulletSpeed)
                        {
                            ac.useSequenceSpeed = EditorGUILayout.Toggle("Use Sequence Speed", ac.useSequenceSpeed);
                        }
                        EditorGUILayout.EndHorizontal();
                        if(ac.overwriteBulletSpeed && !ac.useSequenceSpeed)
                        {
                            ac.speed = AttackPatternPropertyField("Speed", ac.speed, false);
                        }
                        
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
                        if (!ac.passParam)
                        {
                            ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
                        }
                        EditorGUILayout.EndHorizontal();    
                        if (ac.passParam)
                        {
                            ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
                        }
                        ac.bulletTagIndex = EditorUtils.NamedObjectPopup("Bullet Tag", aptew.currentAp.bulletTags, ac.bulletTagIndex, "Bullet Tag");
                        break;
                        
                    case(FireActionType.CallFireTag):
                        ac.fireTagIndex = EditorUtils.NamedObjectPopup("Fire Tag", aptew.currentAp.fireTags, ac.fireTagIndex, "Fire Tag");
                        EditorGUILayout.BeginHorizontal();
                        ac.passParam = EditorGUILayout.Toggle("PassParam", ac.passParam);
                        if (!ac.passParam)
                        {
                            ac.passPassedParam = EditorGUILayout.Toggle("PassMyParam", ac.passPassedParam);
                        }
                        EditorGUILayout.EndHorizontal();    
                        if (ac.passParam)
                        {
                            ac.paramRange = EditorGUILayout.Vector2Field("Param Range", ac.paramRange);
                        }
                        break;
                        
                    case(FireActionType.Repeat):
                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
                        if (foldouts.children [i] == null)
                        {
                            foldouts.children [i] = new FoldoutTreeNode();
                        }
                        NestedFireActionsGUI(ac, foldouts.children [i]);
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        EditorUtils.MoveRemoveAdd<FireAction>(moveRemove, actions, foldouts);
        
        return actions.ToArray();
    }
    
    private static AttackPatternProperty AttackPatternPropertyField(string propName, AttackPatternProperty app, bool isInt)
    {
        EditorGUILayout.BeginHorizontal();
        if (!app.random)
        {
            if(isInt)
            {
                app.FixedValue = (float)EditorGUILayout.IntField(propName, (int)app.FixedValue);
            }
            else
            {
                app.FixedValue = EditorGUILayout.FloatField(propName, app.FixedValue);
            }
        } 
        else
        {
            app.RandomRange = EditorGUILayout.Vector2Field(propName + " Range", app.RandomRange);
        }
        if (isInt)
        {
            app.random = false;
        }
        else
        {
            app.random = EditorGUILayout.Toggle("Randomize", app.random);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        app.rank = EditorGUILayout.Toggle("Add Rank", app.rank);
        if (app.rank)
        {
            app.RankParam = EditorGUILayout.FloatField("Rank Increase", app.RankParam);
        }
        EditorGUILayout.EndHorizontal();
        return app;
    }
    
    private static void ResetFoldouts(FoldoutTreeNode foldouts)
    {
        ResetFoldouts(foldouts, false);
    }
    
    private static void ResetFoldouts(FoldoutTreeNode foldouts, bool zeroed)
    {
        foldouts.children.Clear();
        foldouts.foldouts.Clear();
        if(!zeroed)
        {
            foldouts.children.Add(null);
            foldouts.foldouts.Add(true);
        }
    }

    public Vector3 UpDownRemoveButtons(Vector3 moveRemove, int count, int i)
    {
        return UpDownRemoveButtons(moveRemove, count, i, false);
    }
    
    public Vector3 UpDownRemoveButtons(Vector3 moveRemove, int count, int i, bool zeroed)
    {
        GUI.enabled = (i > 0);
        if (GUILayout.Button('\u25B2'.ToString(), GUILayout.Width(22)))
        {
            moveRemove.x = i;       //Move Index
            moveRemove.z = -1f;     //Move Direction
        }
        GUI.enabled = (i < count - 1);
        if (GUILayout.Button('\u25BC'.ToString(), GUILayout.Width(22)))
        {
            moveRemove.x = i;       //Move Index
            moveRemove.z = 1f;      //Move Direction
        }
        GUI.enabled = (count > 1) || zeroed;
        if (GUILayout.Button("X", GUILayout.Width(22)))
        {
            moveRemove.y = i;       //Remove Index
        }
        GUI.enabled = true;
        return moveRemove;
    }
}


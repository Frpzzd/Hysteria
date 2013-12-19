using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackPatternActionEditorWindow : EditorWindow
{
    private Vector2 actionScroll;
    public AttackPatternTagEditorWindow aptew;
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
                BulletActionsGUI(tag as BulletTag);
            } 
            else if(tag is FireTag)
            {
                FireActionsGUI(tag as FireTag);
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
        if (action.type != FireAction.Type.Repeat && action.nestedActions != null)
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
        if (action.type != BulletAction.Type.Repeat && action.nestedActions != null)
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

    private void BulletActionsGUI(BulletTag bulletTag)
    {
        if (bulletTag.actions == null || bulletTag.actions.Length == 0)
        {
            bulletTag.actions = new BulletAction[0];
        }
        
        EditorGUILayout.LabelField("Bullet Tag: " + bulletTag.Name);
        bulletTag.speed = AttackPatternPropertyField("Speed", bulletTag.speed, false);
        
        ExpandCollapseButtons<BulletAction, BulletAction.Type>("Actions", bulletTag.actions);

        bulletTag.actions = BulletActionGUI(bulletTag.actions);   
    }
    
    private void NestedBulletActionsGUI(BulletAction ba)
    {
        if (ba.nestedActions == null || ba.nestedActions.Length == 0)
        {
            ba.nestedActions = new BulletAction[1];
            ba.nestedActions [0] = new BulletAction();
        }
        
        ba.nestedActions = BulletActionGUI(ba.nestedActions);
    }
    
    private void FireActionsGUI(FireTag fireTag)
    {
        if (fireTag.actions == null || fireTag.actions.Length == 0)
        {
            fireTag.actions = new FireAction[1];
            fireTag.actions [0] = new FireAction();
        }

        ExpandCollapseButtons<FireAction, FireAction.Type>("Fire Tag: " + fireTag.name, fireTag.actions);
        
        fireTag.actions = FireActionGUI(fireTag.actions); 
    }
    
    private void NestedFireActionsGUI(FireAction fa)
    {
        if (fa.nestedActions == null || fa.nestedActions.Length == 0)
        {
            fa.nestedActions = new FireAction[1];
            fa.nestedActions [0] = new FireAction();
        }
        
        fa.nestedActions = FireActionGUI(fa.nestedActions);
    }
    
    private BulletAction[] BulletActionGUI(BulletAction[] bulletActions)
    {
        List<BulletAction> actions = new List<BulletAction>(bulletActions);
        
        Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
        for (int i = 0; i < actions.Count; i++)
        {
            Rect boundingRect = EditorGUILayout.BeginVertical();
            GUI.Box(boundingRect, "");
            EditorGUILayout.BeginHorizontal();
            actions[i].foldout = EditorGUILayout.Foldout(actions[i].foldout, "Action " + (i + 1));
            moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i, true);
            EditorGUILayout.EndHorizontal();
            if (actions[i].foldout)
            {
                EditorGUI.indentLevel++;
                BulletAction ac = actions [i];
                ac.type = (BulletAction.Type)EditorGUILayout.EnumPopup("Action Type", ac.type);
                
                switch (ac.type)
                {
                    case(BulletAction.Type.Wait):
                        ac.wait = AttackPatternPropertyField("Wait", ac.wait, false);
                        break;

                    case(BulletAction.Type.ChangeDirection):
                        ac.direction = (DirectionType)EditorGUILayout.EnumPopup("DirectionType", ac.direction);
                        ac.angle = AttackPatternPropertyField("Angle", ac.angle, false);
                        ac.wait = AttackPatternPropertyField("Time", ac.wait, false);
                        ac.waitForChange = EditorGUILayout.Toggle("Wait To Finish", ac.waitForChange);
                        break;
                        
                    case(BulletAction.Type.ChangeSpeed):
                    case(BulletAction.Type.VerticalChangeSpeed):
                        ac.speed = AttackPatternPropertyField("Speed", ac.speed, false);
                        ac.wait = AttackPatternPropertyField("Time", ac.wait, false);
                        ac.waitForChange = EditorGUILayout.Toggle("Wait To Finish", ac.waitForChange);
                        break;

                    case(BulletAction.Type.Repeat):
                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
                        NestedBulletActionsGUI(ac);
                        break;

                    case(BulletAction.Type.Fire):    
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
        EditorUtils.MoveRemoveAdd(moveRemove, actions);
        return actions.ToArray();   
    }
    
    private FireAction[] FireActionGUI(FireAction[] fireActions)
    {
        List<FireAction> actions = new List<FireAction>(fireActions);
        
        Vector3 moveRemove = new Vector3(-1f, -1f, 0f);
        for (int i = 0; i < actions.Count; i++)
        {
            Rect boundingRect = EditorGUILayout.BeginVertical();
            GUI.Box(boundingRect, "");
            EditorGUILayout.BeginHorizontal();
            actions[i].foldout = EditorGUILayout.Foldout(actions[i].foldout, "Action " + (i + 1));
            moveRemove = UpDownRemoveButtons(moveRemove, actions.Count, i);
            EditorGUILayout.EndHorizontal();
            if (actions[i].foldout)
            {
                EditorGUI.indentLevel++;    
                FireAction ac = actions [i];

                ac.type = (FireAction.Type)EditorGUILayout.EnumPopup("Action Type", ac.type);
                
                switch (ac.type)
                {
                    case(FireAction.Type.Wait):
                        ac.wait = AttackPatternPropertyField("Wait", ac.wait, false);
                        break;

                    case(FireAction.Type.Fire):
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

                    case(FireAction.Type.CallFireTag):
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

                    case(FireAction.Type.Repeat):
                        ac.repeat = AttackPatternPropertyField("Repeat", ac.repeat, true);
                        NestedFireActionsGUI(ac);
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        EditorUtils.MoveRemoveAdd<FireAction>(moveRemove, actions);
        
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

    public static void ExpandCollapseButtons<T, P>(string label, NestedAction<T, P>[] actions) where T : NestedAction<T, P>
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        if (GUILayout.Button("Expand All", GUILayout.Width(80)))
        {
            for(int i = 0; i < actions.Length; i++)
            {
                actions[i].Expand(true);
            }
        }
        if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
        {
            for(int i = 0; i < actions.Length; i++)
            {
                actions[i].Expand(true);
            }
        }
        EditorGUILayout.EndHorizontal();
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
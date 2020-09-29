using SoundActor;
using Uniarts;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(ControlDataManager))]
public class ControlDataSenderEditor : Editor
{
    ControlDataManager m_target;

    public override void OnInspectorGUI()
    {
        m_target = (ControlDataManager)target;
        base.OnInspectorGUI();
        EditorGUILayout.Space(8);
        DrawAddControlPointButton();

        for (int i = 0; i < m_target.controlPoints.Count; ++i)
        {
            DrawControlPoint(i);
        }
    }

    void DrawAddControlPointButton()
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add new Control Point", GUILayout.Height(30)))
        {
            //Create an Undo/Redo step for this modification
            Undo.RecordObject(m_target, "Add new State");

            m_target.controlPoints.Add(new AudioControlPoint());

            //Whenever you modify a component variable directly without using serializedObject you need to tell
            //Unity that this component has changed so the values are saved next time the user saves the project.
            EditorUtility.SetDirty(m_target);
        }
        GUI.backgroundColor = Color.white;
    }

    void DrawControlPoint(int index)
    {
        if (index < 0 || index >= m_target.controlPoints.Count)
        {
            return;
        }

        //GUI properties
        GUIStyle cpStyle= new GUIStyle("box");
        cpStyle.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.2f));

        EditorGUILayout.BeginVertical(cpStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.white;
                GUILayout.Label("Type", EditorStyles.label, GUILayout.Width(50));

                //BeginChangeCheck() is a useful way to see if an inspector variable was changed between
                //BeginChangeCheck() and EndChangeCheck()
                //EditorGUI.BeginChangeCheck();
                AudioControlPoint acpoint = m_target.controlPoints[index];
                acpoint.m_controlType  = (ControlDataType)EditorGUILayout.EnumPopup(acpoint.m_controlType);
                GUILayout.FlexibleSpace();
                acpoint.m_active = (bool)EditorGUILayout.Toggle("In use", acpoint.m_active);
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove")) {
                    Undo.RecordObject(m_target, "Delete Control Point");
                    m_target.controlPoints.RemoveAt(index);
                    EditorUtility.SetDirty(m_target);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                //EditorGUILayout.Space(8);

                using (new EditorGUI.IndentLevelScope())
                acpoint.m_foldout = EditorGUILayout.Foldout(acpoint.m_foldout, "Detailed settings");

            if (acpoint.m_foldout) {
                EditorGUILayout.BeginVertical("box");
                if (acpoint.m_controlType == ControlDataType.OSC) {
                    EditorGUILayout.BeginVertical();
                        acpoint.m_oscCommand = EditorGUILayout.TextField("OSC commmand", acpoint.m_oscCommand);
                        acpoint.m_argumentType = (ArgumentType)EditorGUILayout.EnumPopup("Command data", acpoint.m_argumentType);
                        if (acpoint.m_argumentType == ArgumentType.Distance) {
                            EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Distance between:", GUILayout.MaxWidth(125));
                                acpoint.m_bonePointFrom = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointFrom, GUILayout.ExpandWidth(true));
                                acpoint.m_bonePointTo = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointTo, GUILayout.ExpandWidth(true));
                            EditorGUILayout.EndHorizontal();
                        } else
                        {
                            acpoint.m_bonePoint = (HumanBodyBones)EditorGUILayout.EnumPopup("Control track point", acpoint.m_bonePoint);
                        }
                    EditorGUILayout.EndVertical();

                }
                if (acpoint.m_controlType == ControlDataType.FMODEvent) {
                    EditorGUILayout.BeginVertical();
                    m_target.fmodEvent = (string)EditorGUILayout.TextField("Shared fmod event:", m_target.fmodEvent); 
                    acpoint.m_fmodParameter = (string)EditorGUILayout.TextField("Controlled fmod parameter:", acpoint.m_fmodParameter);
                    acpoint.m_argumentType = (ArgumentType)EditorGUILayout.EnumPopup("Value from:", acpoint.m_argumentType);
                        if(acpoint.m_argumentType == ArgumentType.Position)
                        {
                            EditorGUILayout.BeginHorizontal();
                                acpoint.m_axis = (Axis)EditorGUILayout.EnumPopup("Selected axis:", acpoint.m_axis);
                            EditorGUILayout.EndHorizontal();
                             acpoint.m_bonePoint = (HumanBodyBones)EditorGUILayout.EnumPopup("Control track point", acpoint.m_bonePoint);
                        }
                        else if (acpoint.m_argumentType == ArgumentType.Velocity)
                        {
                            acpoint.m_bonePoint = (HumanBodyBones)EditorGUILayout.EnumPopup("Control track point", acpoint.m_bonePoint);
                        }
                    
                if (acpoint.m_argumentType == ArgumentType.Distance)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Distance between:", GUILayout.MaxWidth(125));
                            acpoint.m_bonePointFrom = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointFrom, GUILayout.ExpandWidth(true));
                            acpoint.m_bonePointTo = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointTo, GUILayout.ExpandWidth(true));
                            EditorGUILayout.EndHorizontal();
                        }
                EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space(25);
                EditorGUILayout.BeginHorizontal();
                    acpoint.m_visualizeBonePoint = EditorGUILayout.Toggle("Show joint (scene only)", acpoint.m_visualizeBonePoint);
                    GUILayout.FlexibleSpace();
                    acpoint.m_drawColor = EditorGUILayout.ColorField("Color", acpoint.m_drawColor);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                    acpoint.m_showValue = EditorGUILayout.Toggle("Show value", acpoint.m_showValue);
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Reset min/max"))
                {
                    acpoint.ResetMinMax();
                }
                EditorGUILayout.EndVertical();
                }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }


    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }


    //to visualize data on scenen view
    void OnSceneGUI()
    {
        if (m_target != null)
        {
            for (int i = 0; i < m_target.controlPoints.Count; ++i)
            {
                AudioControlPoint acpoint = m_target.controlPoints[i];

                if (acpoint.valuesShouldBeVisible())
                {
                    //draw the value
                    Handles.BeginGUI();
                    GUI.backgroundColor = acpoint.m_drawColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(MakeTex(15, 15, acpoint.m_drawColor));
                    GUILayout.Box("Value for " + acpoint.m_argumentType.ToString() + ": " + acpoint.fmodParameterValue.ToString("F2"), GUILayout.Width(160));
                    GUILayout.Box("min: " + acpoint.minVal.ToString("F2"), GUILayout.Width(80));
                    GUILayout.Box("max " + acpoint.maxVal.ToString("F2"), GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                    Handles.EndGUI();
                }
            }

        }
    }
}

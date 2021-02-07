using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using SoundActor;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(AudioControlPointController))]
public class AudioPointControllerEditor : Editor
{
    AudioControlPointController m_target;
    private Dictionary<Color, Texture2D> _labelTextureStorage;
    private Dictionary<String, Texture2D> _inspectorTextureStorage;

    public override void OnInspectorGUI()
    {
        if (_labelTextureStorage == null) {_labelTextureStorage = new Dictionary<Color, Texture2D>();}
        if (_inspectorTextureStorage == null) {_inspectorTextureStorage = new Dictionary<String, Texture2D>();}
        m_target = (AudioControlPointController)target;
        //base.OnInspectorGUI();
        
        EditorGUILayout.Space(8);
        DrawControlPointHeader();
        DrawConnectionSetups();
        //EditorGUILayout.Space(8);
        m_target.m_controlPointsFoldout = EditorGUILayout.Foldout(m_target.m_controlPointsFoldout, "Control points");
        if (m_target.m_controlPointsFoldout)
        {
            DrawAddControlPointButton();
            for (int i = 0; i < m_target.controlPoints.Count; ++i)
            {
                DrawControlPoint(i);
            }
        }
        EditorGUILayout.Space(4);
    }

    private void DrawControlPointHeader()
    {
        GUIStyle header = new GUIStyle();
        header.richText = true;
        header.alignment = TextAnchor.LowerCenter;
        EditorGUILayout.BeginHorizontal();
        if (!String.IsNullOrEmpty(m_target.fmodEvent))
        {
            GUILayout.Label($"<size=20><color=#00FF00><b>{m_target.fmodEvent.Split('/')[1]}</b></color></size>", header);   
        } else if(String.IsNullOrEmpty(m_target.oscAddress))
        {
            GUILayout.Label($"<size=12><color=#dddddd><b>please set FMOD event or OSC address</b></color></size>", header);
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawConnectionSetups()
    {
        m_target.m_connectionFoldout = EditorGUILayout.Foldout(m_target.m_connectionFoldout, "Connection and output settings");
        using (new EditorGUI.IndentLevelScope())
            if (m_target.m_connectionFoldout)
            {
                EditorGUILayout.BeginVertical();
                m_target.fmodEvent = (string)EditorGUILayout.TextField("FMOD event path:", m_target.fmodEvent);
                m_target.oscAddress =(string)EditorGUILayout.TextField("OSC ip address:", m_target.oscAddress);
                m_target.oscPort = (int)EditorGUILayout.IntField("OSC port:", m_target.oscPort);
                m_target.m_outputChoice = (OutputChoice)EditorGUILayout.EnumPopup("Value if same targets: ", m_target.m_outputChoice);
                EditorGUILayout.EndVertical();
                m_target.fps = (int)EditorGUILayout.IntField("fps for data: ", m_target.fps);
            }
    }

    void DrawAddControlPointButton()
    {
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add new Control Point", GUILayout.Height(30)))
        {
            //Create an Undo/Redo step for this modification
            Undo.RecordObject(m_target, "Add new State");

            //move this into main class?
            //m_target.controlPoints.Add(new AudioControlPoint());
            m_target.AddNewControlPoint();

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
        AudioControlPoint acpoint = m_target.controlPoints[index];
        
        //GUI properties
        GUIStyle cpStyle= new GUIStyle("box");
        
        //this is bit hacky bit otherwise texture garbage keeps piling up until playmode is exited
        if (!_inspectorTextureStorage.ContainsKey("drawControlPoint"))
        {
            _inspectorTextureStorage["drawControlPoint"] = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.3f));
        }
        cpStyle.normal.background = _inspectorTextureStorage["drawControlPoint"];
        EditorGUILayout.BeginVertical(cpStyle);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.white;
        GUILayout.Label("Control", EditorStyles.label);
        acpoint.m_controlType  = (ControlDataType)EditorGUILayout.EnumPopup(acpoint.m_controlType); 
        // EditorGUI.BeginChangeCheck();
        // if(EditorGUI.EndChangeCheck())
        // {
        // }
        
        EditorGUILayout.Space(15);
        GUIStyle header = new GUIStyle();
        header.richText = true;
        header.alignment = TextAnchor.LowerCenter;
        GUILayout.Label($"<size=15><color=#eeeeee><b>{acpoint.m_fmodParameter}</b></color></size>", header);
        EditorGUILayout.Space(25);
        
        acpoint.m_active = (bool)EditorGUILayout.Toggle(acpoint.m_active);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Remove", GUILayout.Width(90))) {
            Undo.RecordObject(m_target, "Delete Control Point");
            m_target.controlPoints.RemoveAt(index);
            EditorUtility.SetDirty(m_target);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        using (new EditorGUI.IndentLevelScope())
            acpoint.m_foldout = EditorGUILayout.Foldout(acpoint.m_foldout, "Detailed settings (" + acpoint.m_controlPointType.ToString() + ")" );

        if (acpoint.m_foldout) {
            EditorGUILayout.BeginVertical("box");
                
            // OSC SETUP
            if (acpoint.m_controlType == ControlDataType.OSC) {
                EditorGUILayout.BeginVertical();
                acpoint.m_oscCommand = EditorGUILayout.TextField("OSC commmand", acpoint.m_oscCommand);
                DrawControlPointData(acpoint);
                EditorGUILayout.EndVertical();
            }
                
            // FMOD SETUP
            if (acpoint.m_controlType == ControlDataType.FMODEvent) {
                EditorGUILayout.BeginVertical();
                acpoint.m_fmodParameter = (string)EditorGUILayout.TextField("fmod control parameter:", acpoint.m_fmodParameter);
                DrawControlPointData(acpoint);
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

    void DrawControlPointData(AudioControlPoint acpoint)
    {
        acpoint.m_argumentType = (ArgumentType)EditorGUILayout.EnumPopup("Value from:", acpoint.m_argumentType);
        // Position
        if(acpoint.m_argumentType == ArgumentType.Position)
        {
            EditorGUILayout.BeginHorizontal();
            acpoint.m_axis = (Axis)EditorGUILayout.EnumPopup("Selected axis:", acpoint.m_axis);
            EditorGUILayout.EndHorizontal();
            if (acpoint.m_controlPointType == ControlPointType.Humanoid)
            {
                acpoint.m_bonePoint = (HumanBodyBones)EditorGUILayout.EnumPopup("Control track point", acpoint.m_bonePoint);   
            }
        }
        // Velocity
        else if (acpoint.m_argumentType == ArgumentType.Velocity && acpoint.m_controlPointType == ControlPointType.Humanoid)
        {
            acpoint.m_bonePoint = (HumanBodyBones)EditorGUILayout.EnumPopup("Control track point", acpoint.m_bonePoint);
        }
        // Distance
        if (acpoint.m_argumentType == ArgumentType.Distance && acpoint.m_controlPointType == ControlPointType.Humanoid)
        {
            acpoint.m_distanceTo = (DistanceTo) EditorGUILayout.EnumPopup("Distance between:", acpoint.m_distanceTo);
            if (acpoint.m_distanceTo == DistanceTo.ThisToJoint)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Distance between:", GUILayout.MaxWidth(125));
                acpoint.m_bonePointFrom = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointFrom, GUILayout.ExpandWidth(true));
                acpoint.m_bonePointTo = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointTo, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            } else if (acpoint.m_distanceTo == DistanceTo.ThisToObject)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("From joint to object:", GUILayout.MaxWidth(125));
                acpoint.m_bonePointFrom = (HumanBodyBones)EditorGUILayout.EnumPopup(acpoint.m_bonePointFrom, GUILayout.ExpandWidth(true));
                acpoint.m_distanceToGameObject = (GameObject)EditorGUILayout.ObjectField(acpoint.m_distanceToGameObject, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }
        }
        else if (acpoint.m_argumentType == ArgumentType.Distance && acpoint.m_controlPointType == ControlPointType.GameObject)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Distance to object:");
            acpoint.m_distanceToGameObject = (GameObject)EditorGUILayout.ObjectField(acpoint.m_distanceToGameObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        var texture = new Texture2D(width, height);
        texture.hideFlags = HideFlags.HideAndDontSave;
        Color[] pixels = Enumerable.Repeat(col, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
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
                    if (!_labelTextureStorage.ContainsKey(acpoint.m_drawColor))
                    {
                        _labelTextureStorage[acpoint.m_drawColor] = MakeTex(15, 15, acpoint.m_drawColor);
                    }
                    //draw the value
                    Handles.BeginGUI();
                    GUI.backgroundColor = acpoint.m_drawColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(_labelTextureStorage[acpoint.m_drawColor]);
                    GUILayout.Box("Value for " + acpoint.m_argumentType.ToString() + ": " + acpoint.ControlPointDataValue.ToString("F2"), GUILayout.Width(160));
                    GUILayout.Box("min: " + acpoint.minVal.ToString("F2"), GUILayout.Width(80));
                    GUILayout.Box("max " + acpoint.maxVal.ToString("F2"), GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                    Handles.EndGUI();
                }
            }

        }
    }

    private void OnDisable()
    {
        _inspectorTextureStorage.Clear();
        _labelTextureStorage.Clear();
    }
}



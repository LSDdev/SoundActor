using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoundActor
{
    [ExecuteInEditMode]
    public class ControlDataManager : MonoBehaviour
    {
        private FMOD.Studio.EventInstance _instance;
        [HideInInspector]
        public string fmodEvent;
        [HideInInspector]
        public List<AudioControlPoint> controlPoints = new List<AudioControlPoint>();
        [HideInInspector]
        public Animator animator;

        private float _lastExecTime = 0f;
        private float timeThreshold = 0.02f; //50fps
        private GameObject _dataDisplayParent;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
            _instance.start();

            _dataDisplayParent = GameObject.FindGameObjectWithTag("DataDisplay");
            if (!_dataDisplayParent) Debug.LogError("Canvas object with DataDisplay tag set could not be found. No info on data values can be shown. Add canvas element with 'Vertical Layout Group' component and set its tag to DataDisplay.");
        }


        void SendOSC(AudioControlPoint acp)
        {

        }

        void UpdateFMOD(AudioControlPoint acp)
        {
            Vector3 currentPos = animator.GetBoneTransform(acp.m_bonePoint).position;
            float value = 0;
            if (acp.m_argumentType == ArgumentType.Position)
            {
                switch (acp.m_axis)
                {
                    case Axis.x:
                        value = animator.GetBoneTransform(acp.m_bonePoint).position.x;
                        acp.positionOnSelectedAxis = value; //store the result into instance for reuse in other parts of GUI drawing
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.fmodParameterValue = value;
                        break;
                    case Axis.y:
                        value = animator.GetBoneTransform(acp.m_bonePoint).position.y;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.fmodParameterValue = value;
                        break;
                    case Axis.z:
                        value = animator.GetBoneTransform(acp.m_bonePoint).position.z;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.fmodParameterValue = value;
                        break;
                    default:
                        break;
                }
            } else if (acp.m_argumentType == ArgumentType.Velocity)
            {
                Vector3 currFrameVelocity = (currentPos - acp.previousPosition) / Time.deltaTime;
                acp.frameVelocity = Vector3.Lerp(acp.frameVelocity, currFrameVelocity, 0.1f);
                acp.previousPosition = currentPos;
                value = acp.velocityMagnitude();
                //acp.fmodParameterValue = Mathf.Abs(value);
                acp.fmodParameterValue = value;
            } else
            {
                value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, animator.GetBoneTransform(acp.m_bonePointTo).position);
                acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                //acp.fmodParameterValue = Mathf.Abs(value);
                acp.fmodParameterValue = value;
            }
            //set the data on fmod
            _instance.setParameterByName(acp.m_fmodParameter, acp.fmodParameterValue );
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

        private void OnDrawGizmos()
        {
            foreach (AudioControlPoint acp in controlPoints)
            {
                if (acp.m_visualizeBonePoint && acp.m_argumentType != ArgumentType.Distance && acp.m_active)
                {
                    Gizmos.color = acp.m_drawColor;
                    Vector3 pos = animator.GetBoneTransform(acp.m_bonePoint).position;
                    Gizmos.DrawSphere(pos, .03f);
                    Handles.Label(pos + Vector3.up * 0.2f, "Value");
                }
                if (acp.m_visualizeBonePoint && acp.m_argumentType == ArgumentType.Distance && acp.m_active)
                {
                    Gizmos.color = acp.m_drawColor;
                    Gizmos.DrawSphere(animator.GetBoneTransform(acp.m_bonePointFrom).position, .03f);
                    Gizmos.DrawSphere(animator.GetBoneTransform(acp.m_bonePointTo).position, .03f);
                    var p1 = animator.GetBoneTransform(acp.m_bonePointFrom).position;
                    var p2 = animator.GetBoneTransform(acp.m_bonePointTo).position;
                    Handles.DrawBezier(p1, p2, p1, p2, acp.m_drawColor, null, 8f);
                }

            }
        }

        private void Update()
        {
            foreach (AudioControlPoint acp in controlPoints)
            {
                if (Time.time - _lastExecTime > timeThreshold)
                { // let's throttle a bit the not so optimized loops
                    if (acp.m_controlType == ControlDataType.OSC && acp.m_active) SendOSC(acp);
                    if (acp.m_controlType == ControlDataType.FMODEvent && acp.m_active) UpdateFMOD(acp);
                }
            }
            _lastExecTime = Time.time;
        }
    
    }
}
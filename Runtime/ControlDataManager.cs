using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;
using UnityEditor;
using OscJack;
using Debug = UnityEngine.Debug;

namespace SoundActor
{
    [ExecuteInEditMode]
    public class ControlDataManager : MonoBehaviour
    {
        private FMOD.Studio.EventInstance _instance;
        [HideInInspector]
        public string fmodEvent;
        [HideInInspector]
        public string oscAddress;
        [HideInInspector]
        public int oscPort;
        [HideInInspector]
        public bool m_connectionFoldout = true;
        [HideInInspector]
        public bool m_controlPointsFoldout = true;
        [HideInInspector]
        public List<AudioControlPoint> controlPoints = new List<AudioControlPoint>();
        [HideInInspector]
        public Animator animator;

        private float _lastExecTime = 0f;
        private float _timeThreshold = 0.02f; //50fps
        private float _fmodExectime;
        private OscClient _client;
        
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (!String.IsNullOrEmpty(fmodEvent))
            {
                _instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
                _instance.start();   
            }

            if (!String.IsNullOrEmpty(oscAddress) && oscPort > 1000)  //FIXME: port checking is just wrong
            {
                _client = new OscClient(oscAddress, oscPort);
            }
        }


        void SendOSC(AudioControlPoint acp)  //NEED TO REFACTOR THE PART BELOW TOGETHER WITH FMOD
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
            } else if (acp.m_argumentType == ArgumentType.Distance)
            {
                if (acp.m_distanceTo == DistanceTo.JointToJoint)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, animator.GetBoneTransform(acp.m_bonePointTo).position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.fmodParameterValue = value;
                } else if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.JointToObject)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, acp.m_distanceToGameObject.transform.position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.fmodParameterValue = value;
                }
            }

            _client.Send(acp.m_oscCommand, acp.fmodParameterValue); 
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
            } else if (acp.m_argumentType == ArgumentType.Distance)
            {
                if (acp.m_distanceTo == DistanceTo.JointToJoint)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, animator.GetBoneTransform(acp.m_bonePointTo).position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.fmodParameterValue = value;
                } else if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.JointToObject)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, acp.m_distanceToGameObject.transform.position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.fmodParameterValue = value;
                }
                
            }
            //set the data on fmod
            FMOD.RESULT _result = _instance.setParameterByName(acp.m_fmodParameter, acp.fmodParameterValue );
            //Debug.Assert(_result == RESULT.OK, "Couldn't set the volume");
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
                    Vector3 p1, p2 = Vector3.zero;
                    Gizmos.color = acp.m_drawColor;
                    
                    p1 = animator.GetBoneTransform(acp.m_bonePointFrom).position;
                    Gizmos.DrawSphere(p1, .03f);
                    
                    if (acp.m_distanceTo == DistanceTo.JointToJoint)
                    {
                        p2 = animator.GetBoneTransform(acp.m_bonePointTo).position;
                        Gizmos.DrawSphere(p2, .03f);
                    } 
                    else if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.JointToObject)
                    {
                        p2 = acp.m_distanceToGameObject.transform.position;
                        Gizmos.DrawSphere(p2, .03f);
                    }

                    if (acp.m_distanceTo == DistanceTo.JointToJoint || acp.m_distanceTo == DistanceTo.JointToObject && acp.m_distanceToGameObject != null)  // make sure relevant data is available and draw
                    {
                        Handles.DrawBezier(p1, p2, p1, p2, acp.m_drawColor, null, 8f);    
                    }
                }

            }
        }

        private void Update()
        {
            if (Time.time - _lastExecTime > _timeThreshold)
            {
                foreach (AudioControlPoint acp in controlPoints)
                {
                    // let's throttle a bit the not so optimized loops
                    if (acp.m_controlType == ControlDataType.OSC && acp.m_active) SendOSC(acp);
                    if (acp.m_controlType == ControlDataType.FMODEvent && acp.m_active) UpdateFMOD(acp);
                }
                _lastExecTime = Time.time;
            }
        }
    
    }
}
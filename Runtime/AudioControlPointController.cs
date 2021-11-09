using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using UnityEditor;
using OscJack;
using Debug = UnityEngine.Debug;

namespace SoundActor
{
    [ExecuteAlways]
    public class AudioControlPointController : MonoBehaviour
    {
        private FMOD.Studio.EventInstance _instance;
        [HideInInspector]
        public string fmodEvent;
        [HideInInspector]
        public string oscAddress;
        [HideInInspector]
        public int oscPort = 9200;
        [HideInInspector]
        public OutputChoice m_outputChoice;
        [HideInInspector]
        public bool m_connectionFoldout = true;
        [HideInInspector]
        public bool m_startModeFoldout = true;
        [HideInInspector]
        public bool m_stopModeFoldout = false;
        [HideInInspector]
        public bool m_controlPointsFoldout = true;
        [HideInInspector]
        public bool m_miscSettingsFoldout = false;
        [HideInInspector]
        public List<AudioControlPoint> controlPoints = new List<AudioControlPoint>();
        [HideInInspector]
        public Animator animator;
		[HideInInspector]
        public int fps = 25;
        [HideInInspector]
        public StartMode m_startMode;
        [HideInInspector]
        public StopMode m_stopMode;
        [HideInInspector]
        public string m_startSignalName;
        [HideInInspector]
        public string m_stopSignalName;
        [HideInInspector]
        public Vector3 m_globalPositionOffset = Vector3.zero;

        private float _lastExecTime = 0f;
        private float _timeThreshold;
        private float _fmodExectime;
        private OscClient _client;

        private bool debugMode = true;

        private Dictionary<string, List<AudioControlPoint>> _fmodPointsByAttribute = new Dictionary<string, List<AudioControlPoint>>();
        private Dictionary<string, List<AudioControlPoint>> _oscPointsByCommand = new Dictionary<string, List<AudioControlPoint>>();
        
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable() {
            if (m_startMode == StartMode.Triggered && !String.IsNullOrEmpty(m_startSignalName))
            {
                EventSignaling.StartListening(m_startSignalName, StartFMOD);    
            }
            if (m_stopMode == StopMode.Triggered && !String.IsNullOrEmpty(m_stopSignalName))
            {
                EventSignaling.StartListening(m_stopSignalName, StopFMOD);    
            }
            
        }

        private void Start()
        {
            if (!String.IsNullOrEmpty(fmodEvent))
            {
                //need to have shared instance pointing to fmod event between possible multiple audiopointcontroller
                _instance = FMODEventInstancer.instance.GetFmodEventInstance(fmodEvent);
                if(m_startMode == StartMode.Automatic) { StartFMOD(); }
            }

            if (!String.IsNullOrEmpty(oscAddress) && oscPort > 1000)  //FIXME: port checking this way is just wrong
            {
                _client = new OscClient(oscAddress, oscPort);
            }

            //Need to zero _lastExecTime as it's value is otherwise carried over from edit mode execution
            _lastExecTime = 0f;

            if (debugMode && (controlPoints!= null) && (controlPoints.Count > 0) )
            {
                //Debug.Log("Currently " + controlPoints.Count.ToString() + " parameter control points attached to " + gameObject.name + " for fmod instrument " + fmodEvent.Split('/')[1]);
                Debug.Log("Currently " + controlPoints.Count.ToString() + " parameter control points attached to " + gameObject.name );
            }
        }

        private void StartFMOD() {
            FMOD.Studio.PLAYBACK_STATE ps;
            _instance.getPlaybackState(out ps);
            if(ps != FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                _instance.start();
            }
        }
        
        private void StopFMOD() {
            FMOD.Studio.PLAYBACK_STATE ps;
            _instance.getPlaybackState(out ps);
            if(ps == FMOD.Studio.PLAYBACK_STATE.PLAYING) {
                _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        void UpdateValueHumanoid(AudioControlPoint acp)
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
                        acp.ControlPointDataValue = value;
                        break;
                    case Axis.y:
                        value = animator.GetBoneTransform(acp.m_bonePoint).position.y;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.ControlPointDataValue = value;
                        break;
                    case Axis.z:
                        value = animator.GetBoneTransform(acp.m_bonePoint).position.z;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.ControlPointDataValue = value;
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
                acp.ControlPointDataValue = value;
            } else if (acp.m_argumentType == ArgumentType.Distance)
            {
                if (acp.m_distanceTo == DistanceTo.ThisToJoint)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, animator.GetBoneTransform(acp.m_bonePointTo).position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.ControlPointDataValue = value;
                } else if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.ThisToObject)
                {
                    value = Vector3.Distance(animator.GetBoneTransform(acp.m_bonePointFrom).position, acp.m_distanceToGameObject.transform.position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.ControlPointDataValue = value;
                }
                
            }
        }

        private void UpdateValueGameObject(AudioControlPoint acp)
        {
         Vector3 currentPos = transform.position;
            float value = 0;
            if (acp.m_argumentType == ArgumentType.Position)
            {
                switch (acp.m_axis)
                {
                    case Axis.x:
                        value = transform.position.x - m_globalPositionOffset.x;
                        acp.positionOnSelectedAxis = value; //store the result into instance for reuse in other parts of GUI drawing
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.ControlPointDataValue = value;
                        break;
                    case Axis.y:
                        value = transform.position.y - m_globalPositionOffset.y;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.ControlPointDataValue = value;
                        break;
                    case Axis.z:
                        value = transform.position.z - m_globalPositionOffset.z;
                        acp.positionOnSelectedAxis = value;
                        //acp.fmodParameterValue = Mathf.Abs(value);
                        acp.ControlPointDataValue = value;
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
                acp.ControlPointDataValue = value;
            } else if (acp.m_argumentType == ArgumentType.Distance)
            {
                if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.ThisToObject)
                {
                    value = Vector3.Distance(transform.position, acp.m_distanceToGameObject.transform.position);
                    acp.distanceBetweenPoints = value; //store the result into instance for reuse in other parts of GUI drawing
                    //acp.fmodParameterValue = Mathf.Abs(value);
                    acp.ControlPointDataValue = value;
                }
                
            }
        }

        public void AddNewControlPoint()
        {
            AudioControlPoint acp = new AudioControlPoint();
            //detect is there humanoid rig connected to this game object
            if (animator && animator.GetBoneTransform(acp.m_bonePoint))
            {
                acp.m_controlPointType = ControlPointType.Humanoid;
            }
            else
            {
                acp.m_controlPointType = ControlPointType.GameObject;
            }
            // some defaults based on control point type
            if (acp.m_controlPointType == ControlPointType.GameObject)
            {
                acp.m_distanceTo = DistanceTo.ThisToObject;
            }
            
            controlPoints.Add(acp);
        }

        private void Update()
        {
            _timeThreshold = 1.0f / fps;
            if (Time.time - _lastExecTime > _timeThreshold)   //throttle the data output
            {
                IterateControlPoints();
                SendData(_fmodPointsByAttribute);
                SendData(_oscPointsByCommand);
                _lastExecTime = Time.time;
            }
        }

        private void SendData(Dictionary<string,List<AudioControlPoint>> pointDict)
        {
            //
            foreach (KeyValuePair<string,List<AudioControlPoint>> entry in pointDict)
            {
                var attributeOrCommand = entry.Key;
                var size = entry.Value.Count;
                float averageValue = 0f;
                float maxValue = -10000f;
                foreach (var point in entry.Value)
                {
                    //update value based on object type acp is linked to
                    if (point.m_controlPointType == ControlPointType.Humanoid)
                    {
                        UpdateValueHumanoid(point);    
                    }
                    else
                    {
                        UpdateValueGameObject(point);
                    }
                    
                    averageValue += point.ControlPointDataValue;
                    maxValue = point.ControlPointDataValue > maxValue ? point.ControlPointDataValue : maxValue;
                }

                //determine the final value for output
                float outValue = m_outputChoice == OutputChoice.MaxValue ? maxValue : averageValue / size;
                
                //FMOD or OSC
                if (Application.isPlaying)  //so we are not sending data while in edit
                {
                    if (entry.Value[0].m_controlType == ControlDataType.FMODEvent)
                    {
                        FMOD.RESULT _result = _instance.setParameterByName(attributeOrCommand, outValue);    
                    } else if (entry.Value[0].m_controlType == ControlDataType.OSC)
                    {
                        try
                        {
                            _client.Send(attributeOrCommand, outValue);
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                            {   
                                Debug.Log("Destination address and port are not listening. Check the ip and port number.");
                            }
                        }
                         
                    }
                }
            }
            pointDict.Clear();
        }
        
        private void IterateControlPoints()
        {
            foreach (AudioControlPoint acp in controlPoints) 
            {
                //this is inefficient as we start from beginning on each update loop, but it doesn't matter now in this context
                //create audio control point groupings based on point's current fmod attribute or OSC command
                //use own list for each type, fmod or osc
                //all this is done to handle cases where we have multiple control points to control one and same fmod attribute or osc command
                //on these we can use aggregate calculations to have the final output value from multiple points to one control
                if (acp.m_controlType == ControlDataType.FMODEvent)
                {
                    //is this first occurrance of this fmod attribute as a key in dict
                    if (!_fmodPointsByAttribute.ContainsKey(acp.m_fmodParameter))
                    {
                        var list = new List<AudioControlPoint>();
                        list.Add(acp);
                        _fmodPointsByAttribute[acp.m_fmodParameter] = list;
                    }
                    else
                    {
                        var list = _fmodPointsByAttribute[acp.m_fmodParameter];
                        list.Add(acp);
                        _fmodPointsByAttribute[acp.m_fmodParameter] = list;
                    }
                } else if (acp.m_controlType == ControlDataType.OSC)
                {
                    //is this first occurrance of this OSC command as a key in dict
                    if (!_oscPointsByCommand.ContainsKey(acp.m_oscCommand))
                    {
                        var list = new List<AudioControlPoint>();
                        list.Add(acp);
                        _oscPointsByCommand[acp.m_oscCommand] = list;
                    }
                    else
                    {
                        var list = _oscPointsByCommand[acp.m_oscCommand];
                        list.Add(acp);
                        _oscPointsByCommand[acp.m_oscCommand] = list;
                    }       
                } 
            }
        }
        
        private void OnDrawGizmos()
        {
            foreach (AudioControlPoint acp in controlPoints)
            {
                //Draw highlight only to humanoid joints 
                if (acp.m_visualizeBonePoint && acp.m_argumentType != ArgumentType.Distance && acp.m_controlPointType != ControlPointType.GameObject && acp.m_active)
                {
                    Gizmos.color = acp.m_drawColor;
                    Vector3 pos = animator.GetBoneTransform(acp.m_bonePoint).position;
                    Gizmos.DrawSphere(pos, .05f);
                    Handles.Label(pos + Vector3.up * 0.4f, acp.ControlPointDataValue.ToString("F2"));
                }
                if (acp.m_visualizeBonePoint && acp.m_argumentType == ArgumentType.Distance && acp.m_active)
                {
                    //highlight bonepoint on humanoid, draw bezier on distance of that is the tracked attribute 
                    Vector3 p1, p2 = Vector3.zero;
                    Gizmos.color = acp.m_drawColor;

                    if (acp.m_controlPointType == ControlPointType.Humanoid)
                    {
                        p1 = animator.GetBoneTransform(acp.m_bonePointFrom).position;
                    }
                    else
                    {
                        p1 = transform.position;
                    }
                    
                    Gizmos.DrawSphere(p1, .03f);
                    
                    if (acp.m_distanceTo == DistanceTo.ThisToJoint)
                    {
                        p2 = animator.GetBoneTransform(acp.m_bonePointTo).position;
                        Gizmos.DrawSphere(p2, .03f);
                    } 
                    else if (acp.m_distanceToGameObject != null && acp.m_distanceTo == DistanceTo.ThisToObject)
                    {
                        p2 = acp.m_distanceToGameObject.transform.position;
                        Gizmos.DrawSphere(p2, .03f);
                    }

                    if (acp.m_distanceTo == DistanceTo.ThisToJoint || acp.m_distanceTo == DistanceTo.ThisToObject && acp.m_distanceToGameObject != null)  // make sure relevant data is available and draw
                    {
                        Handles.DrawBezier(p1, p2, p1, p2, acp.m_drawColor, null, 8f);    
                    }
                }
            }
        }

        private void OnDisable()
        {
            EventSignaling.StopListening(m_startSignalName, StartFMOD);
            FMODEventInstancer.instance.ReleaseFmodInstance(fmodEvent);  // This is not okay, need to build some sort of retaining relation from instances hooking into fmod and only release the instance when there is no one left
        }
    }
}

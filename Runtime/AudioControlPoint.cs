using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundActor
{
    [System.Serializable]
    public class AudioControlPoint
    {
        public ControlDataType m_controlType = ControlDataType.FMODEvent;
        public ControlPointType m_controlPointType = ControlPointType.Humanoid;
        public string m_oscCommand = "/";
        public string m_fmodParameter = "";
        public ArgumentType m_argumentType = ArgumentType.Position;
        public Axis m_axis = Axis.x;
        public DistanceTo m_distanceTo = DistanceTo.ThisToJoint;
        public HumanBodyBones m_bonePoint = HumanBodyBones.Neck;
        public HumanBodyBones m_bonePointFrom = HumanBodyBones.LeftHand;
        public HumanBodyBones m_bonePointTo = HumanBodyBones.RightHand;
        
        public GameObject m_distanceToGameObject;
        //public ControlDataManager cds; //reference to the object holding the control point
        public Color m_drawColor = Color.red;
        public Vector3 previousPosition = Vector3.zero;
        public Vector3 frameVelocity = Vector3.zero;
        public float positionOnSelectedAxis = 0f;
        public float distanceBetweenPoints = 0f;

        private float _controlPointDataValue;
        public float ControlPointDataValue {
            get { return _controlPointDataValue; }
            set { _controlPointDataValue = value;
                if(_controlPointDataValue < minVal)
                {
                    minVal = _controlPointDataValue;
                } else if (_controlPointDataValue > maxVal)
                {
                    maxVal = _controlPointDataValue;
                }
            }
        }  //store the current value of parameter to fmod into this, can be reused in different parts of the UI
        public float minVal;
        public float maxVal;

        public virtual float velocityMagnitude()
        {
            return frameVelocity.magnitude;
        }

        public virtual bool valuesShouldBeVisible()
        {
            if (m_active && m_showValue) return true;
            return false;
        }
        
        public bool m_active = true;

        //FIXME: these are for the unity inspector editor system, should be refactored out of here
        public bool m_foldout = true;
        public bool m_visualizeBonePoint = false;
        public bool m_showValue = false;

        public void ResetMinMax()
        {
            minVal = 100f;
            maxVal = -100f;
        }

    }
}
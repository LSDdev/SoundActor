using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public enum ControlDataType
{
    FMODEvent,
    OSC
}

public enum ArgumentType
{
    Position,
    Velocity,
    Distance
}

public enum DistanceTo
{
    JointToJoint,
    JointToObject
}

public enum Axis
{
    x,
    y,
    z
}
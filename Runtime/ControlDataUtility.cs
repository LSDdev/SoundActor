using System;


public enum ControlDataType
{
    FMODEvent,
    OSC
}

public enum ControlPointType
{
    Humanoid,
    GameObject
}

public enum OutputChoice
{
    MaxValue,
    Avaraged
}

public enum ArgumentType
{
    Position,
    Velocity,
    Distance
}

public enum DistanceTo
{
    ThisToJoint,
    ThisToObject
}

public enum Axis
{
    x,
    y,
    z
}

public enum StartMode {
    Automatic,
    Triggered
}

public enum StopMode {
    Automatic,
    Triggered
}
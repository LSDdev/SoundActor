﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FMOD.Studio;

public sealed class FMODEventInstancer
{
    private Dictionary<String, EventInstance> _instances;
    
    
    private static FMODEventInstancer _instance = null;
    // mutex lock for thread safety
    private static readonly object mutex = new object();
    
    FMODEventInstancer()
    {
        _instances = new Dictionary<string, EventInstance>();
    }

    public static FMODEventInstancer instance
    {
        get
        {
            lock (mutex)
            {
                if (_instance == null)
                {
                    _instance = new FMODEventInstancer();
                }
                return _instance;
            }
        }
    }

    public EventInstance GetFmodEventInstance(string eventName)
    {
        if (_instances.Keys.Contains(eventName))
        {
            return _instances[eventName];
        }
        else
        {
            var eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventName);
            eventInstance.start();
            _instances.Add(eventName, eventInstance);
            return eventInstance;
        }
    }

    public void ReleaseFmodInstance(string eventName)
    {
        if (_instances.Keys.Contains(eventName))
        {
            var eventInstance = _instances[eventName];
            eventInstance.release();
            _instances.Remove(eventName);
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventSignaling : MonoBehaviour {

    private Dictionary <string, UnityEvent> eventDictionary;

    private static EventSignaling eventSignaling;

    public static EventSignaling instance
    {
        get
        {
            if (!eventSignaling)
            {
                eventSignaling = FindObjectOfType (typeof (EventSignaling)) as EventSignaling;

                if (!eventSignaling)
                {
                    Debug.LogError ("There needs to be one active EventSignaling script on a GameObject in your scene.");
                }
                else
                {
                    eventSignaling.Init (); 
                }
            }

            return eventSignaling;
        }
    }

    void Init ()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening (string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
        {
            thisEvent.AddListener (listener);
        } 
        else
        {
            thisEvent = new UnityEvent ();
            thisEvent.AddListener (listener);
            instance.eventDictionary.Add (eventName, thisEvent);
        }
    }

    public static void StopListening (string eventName, UnityAction listener)
    {
        if (eventSignaling == null) return;
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
        {
            thisEvent.RemoveListener (listener);
        }
    }

    public static void TriggerEvent (string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
        {
            thisEvent.Invoke ();
        }
    }
}
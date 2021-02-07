using UnityEngine;

public class AudioControlEventStarter : MonoBehaviour
{
    public string signalName;
    
    void OnCollisionEnter(Collision col) {
        EventSignaling.TriggerEvent(signalName);
    }

    void OnTriggerEnter(Collider col) {
        EventSignaling.TriggerEvent(signalName);
    }
}

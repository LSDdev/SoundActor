/* Event signaling pattern, the trigger part to be attached on GameObject to trigger sound control point controller */
using UnityEngine;

public class AudioControlEventControl : MonoBehaviour
{
    public string triggerEnterSignalName;
    public string triggerExitSignalName;
    
    void OnCollisionEnter(Collision col) {
        EventSignaling.TriggerEvent(triggerEnterSignalName);
    }

    void OnTriggerEnter(Collider col) {
        EventSignaling.TriggerEvent(triggerEnterSignalName);
    }
    
    void OnCollisionExit(Collision col) {
        EventSignaling.TriggerEvent(triggerExitSignalName);
    }

    void OnTriggerExit(Collider col) {
        EventSignaling.TriggerEvent(triggerExitSignalName);
    }
}

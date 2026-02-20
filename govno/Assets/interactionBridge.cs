using UnityEngine;

public class PlayerInteractionBridge : MonoBehaviour
{
    private InteractionDetector detector;

    void Awake()
    {
        detector = GetComponentInChildren<InteractionDetector>();
    }

    void OnInteract()
    {
        if (detector != null)
        {
            detector.TriggerInteraction();
        }
    }
}
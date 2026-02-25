using UnityEngine;
using UnityEngine.Events;

public class ObjectButton : MonoBehaviour
{
    public ActivatorType acceptedTypes;

    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    private int validObjectsOnButton = 0;

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<InteractableObject>();
        if (obj == null) return;

        if ((acceptedTypes & obj.activatorType) != 0)
        {
            validObjectsOnButton++;

            if (validObjectsOnButton == 1)
                onActivated.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<InteractableObject>();
        if (obj == null) return;

        if ((acceptedTypes & obj.activatorType) != 0)
        {
            validObjectsOnButton--;

            if (validObjectsOnButton <= 0)
                onDeactivated.Invoke();
        }
    }
}
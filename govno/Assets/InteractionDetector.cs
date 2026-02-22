using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public GameObject interactionIcon;
    public float interactionRadius = 2f;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);

        bool found = false;

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                found = true;
                break;
            }
        }

        interactionIcon.SetActive(found);
    }

    public void TriggerInteraction()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact();
                return;
            }
        }
    }
}
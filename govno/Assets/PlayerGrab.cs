using UnityEngine;

public class PlayerGrabIso : MonoBehaviour
{
    public Transform holdParent;
    public float grabRange = 2f;
    public KeyCode grabKey = KeyCode.E;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private Collider heldCollider;
    private Collider playerCollider;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (HasInteractableNearby())
                return;

            if (heldObject == null)
                TryGrab();
            else
                Drop();
        }

        if (heldObject != null)
            FollowHoldPoint();
    }

    bool HasInteractableNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
                return true;
        }

        return false;
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Grabbable"))
            {
                heldObject = hit.gameObject;
                heldRb = heldObject.GetComponent<Rigidbody>();
                heldCollider = hit;

                if (heldRb != null)
                {
                    heldRb.useGravity = false;
                    heldRb.isKinematic = true;
                }

                // Игнор физики игрока, иначе все уезжает нафиг
                if (playerCollider != null && heldCollider != null)
                    Physics.IgnoreCollision(playerCollider, heldCollider, true);

                return;
            }
        }
    }

    void FollowHoldPoint()
    {
        heldObject.transform.position = holdParent.position;
        heldObject.transform.rotation = transform.rotation;
    }

    void Drop()
    {
        if (heldRb != null)
        {
            heldRb.useGravity = true;
            heldRb.isKinematic = false;
        }

        // Возвращение физики
        if (playerCollider != null && heldCollider != null)
            Physics.IgnoreCollision(playerCollider, heldCollider, false);

        heldObject = null;
        heldRb = null;
        heldCollider = null;
    }
}
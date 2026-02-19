using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    PlayerInput playerInput;
    InputAction interactAction;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput not found anywhere!");
            return;
        }

        interactAction = playerInput.actions.FindAction("Interact");

        if (interactAction == null)
        {
            Debug.LogError("Interact action not found in Input Actions!");
        }
    }


    void OnEnable()
    {
        if (interactAction != null)
            interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.performed -= OnInteract;
    }

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnInteract called");
        interactableInRange?.Interact();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}

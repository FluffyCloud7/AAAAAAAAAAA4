using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static System.Runtime.CompilerServices.RuntimeHelpers;


public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    // void Update()
    //  {
    //    if (UnityEngine.Input.GetKeyDown(KeyCode.E))
    //    {
    //        interactableInRange?.Interact();
    //   }
    // }

    public void OnInteract(InputAction.CallbackContext context)
   {
       if (context.performed)
       {
           interactableInRange?.Interact();
       }
   }

    private void OnTriggerEnter(Collider other) //might be problematic, should check
    {
        if(other.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
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

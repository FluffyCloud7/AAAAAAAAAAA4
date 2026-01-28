using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    public DoorRotation door; // перетащи сюда DoorPivot1
    private bool playerInside = false;
    [SerializeField] string TegPlatforma; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TegPlatforma))
        door.OpenDoor();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TegPlatforma))
        door.CloseDoor();
    }

}

using UnityEngine;
using System.Collections;

public class ButtonUniversal : MonoBehaviour
{
    public DoorRotation door; // дверь, которую открывает кнопка
    private int activatorsCount = 0; // количество объектов на кнопке

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Grabbable"))
        {
            activatorsCount++;
            if (activatorsCount == 1) // первый активатор → открываем дверь
                door.OpenDoor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Grabbable"))
        {
            activatorsCount--;
            if (activatorsCount <= 0) // если никого нет → закрываем дверь
            {
                activatorsCount = 0; // на всякий случай
                door.CloseDoor();
            }
        }
    }
}

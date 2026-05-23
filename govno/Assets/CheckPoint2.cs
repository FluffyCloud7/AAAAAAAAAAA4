using UnityEngine;

public class CheckPoint2 : MonoBehaviour
{
    public CheckPoint2 trigger;

    [Header("Ссылка на спираль")]
    [Tooltip("Перетащи сюда объект SpiralPoint, на котором висит скрипт спирали")]
    public CorrectSpiralFloorText floorSpiral;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Наш базовый рабочий функционал (не трогаем!)
            respawnController.Instance.respawnPoint = transform;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth();
            }

            // АКТИВАЦИЯ ИЗМЕНЕНИЙ В СПИРАЛИ
            if (floorSpiral != null)
            {
                floorSpiral.ActivateCheckpoint();
            }

            trigger.enabled = false;
        }
    }
}
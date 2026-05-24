using UnityEngine;

public class CheckPoint2 : MonoBehaviour
{
    public CheckPoint2 trigger;

    [Header("Ссылка на спираль")]
    [Tooltip("Перетащи сюда объект SpiralPoint, на котором висит скрипт спирали")]
    public CorrectSpiralFloorText floorSpiral;

    [Header("Ссылка на перо")]
    [Tooltip("Перетащи сюда модельку пера, на которой висит скрипт FeatherFloating")]
    public FeatherFloating feather; // ДОБАВЛЕНО: Ссылка на скрипт пера

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

            // АКТИВАЦИЯ СЖАТИЯ ПЕРА
            if (feather != null)
            {
                feather.CollectFeather(); // ДОБАВЛЕНО: Запускаем схлопывание пера
            }

            trigger.enabled = false;
        }
    }

    // ДОБАВЛЕНО: Отрисовка границ коллайдера в окне Scene для удобного центрирования пера
    private void OnDrawGizmos()
    {
        // Проверяем, квадратный ли коллайдер
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.15f); // Прозрачный желтый
            Gizmos.DrawCube(transform.position + box.center, box.size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }

        // Проверяем, сферический ли коллайдер
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.15f); // Прозрачный желтый
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}
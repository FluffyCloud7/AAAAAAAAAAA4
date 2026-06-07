using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    [Header("Связанная дверь")]
    public AdvancedDoorOpener targetDoor; // Сюда перетащить дверь

    [Header("Визуальное нажатие кнопки (Опционально)")]
    public Transform plateMesh;          // Объект самой крышки кнопки, которая уходит вниз
    public float pressDepth = 0.15f;     // На сколько кнопка прожимается вниз
    public float pressSpeed = 5f;

    // Список объектов, которые СЕЙЧАС стоят на кнопке
    private List<Collider> objectsOnPlate = new List<Collider>();

    private Vector3 unpressedLocalPos;
    private Vector3 pressedLocalPos;

    void Start()
    {
        if (plateMesh != null)
        {
            unpressedLocalPos = plateMesh.localPosition;
            pressedLocalPos = unpressedLocalPos + Vector3.down * pressDepth;
        }
    }

    void Update()
    {
        // Плавно двигаем саму модельку кнопки вверх/вниз в зависимости от того, пустая она или нет
        if (plateMesh != null)
        {
            Vector3 targetLocalPos = (objectsOnPlate.Count > 0) ? pressedLocalPos : unpressedLocalPos;
            plateMesh.localPosition = Vector3.Lerp(plateMesh.localPosition, targetLocalPos, Time.deltaTime * pressSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, это Игрок (наличие PlayerGrabIso) или тяжелый Куб (GrabbableItem с типом HeavyInHands)
        bool isPlayer = other.GetComponent<PlayerGrabIso>() != null;

        GrabbableItem item = other.GetComponent<GrabbableItem>();
        bool isHeavyCube = item != null && item.itemSize == ItemSize.HeavyInHands;

        if (isPlayer || isHeavyCube)
        {
            if (!objectsOnPlate.Contains(other))
            {
                objectsOnPlate.Add(other);

                // Если это первый зашедший объект — открываем дверь
                if (objectsOnPlate.Count == 1 && targetDoor != null)
                {
                    targetDoor.TargetOpen();
                    Debug.Log("Кнопка зажата! Дверь открывается.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectsOnPlate.Contains(other))
        {
            objectsOnPlate.Remove(other);

            // Если на кнопке больше никого не осталось — закрываем дверь
            if (objectsOnPlate.Count == 0 && targetDoor != null)
            {
                targetDoor.TargetClose();
                Debug.Log("Кнопка отжата! Дверь закрывается.");
            }
        }
    }
}
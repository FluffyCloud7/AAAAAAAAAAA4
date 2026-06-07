using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvancedDoorOpener : MonoBehaviour, IInteractable
{
    [Header("Режим работы")]
    [Tooltip("Если true - нужны предметы для открытия по кнопке E. Если false - открывается только от кнопки/плиты.")]
    public bool useItemsToOpen = true;
    public List<string> requiredItemIDs; // ID предметов (нужны только еслиuseItemsToOpen = true)

    [Header("Настройки движения")]
    public bool openUpwards = false;     // true - едет вверх, false - вращается
    public float moveAmount = 2.5f;      // Расстояние сдвига
    public float rotationAngle = 90f;    // Угол поворота
    public float openTime = 1.0f;        // Время движения в секундах

    [Header("Эффекты")]
    public ParticleSystem doorParticles;

    private bool isOpen = false;
    private bool isAnimating = false;
    private PlayerGrabIso player;

    private Vector3 closedPos;
    private Quaternion closedRot;
    private Vector3 openPos;
    private Quaternion openRot;
    private Coroutine movementCoroutine;

    void Start()
    {
        player = FindAnyObjectByType<PlayerGrabIso>();

        // Запоминаем начальные и конечные точки один раз при старте
        closedPos = transform.position;
        closedRot = transform.rotation;

        openPos = openUpwards ? closedPos + Vector3.up * moveAmount : closedPos;
        openRot = openUpwards ? closedRot : closedRot * Quaternion.Euler(0, rotationAngle, 0);

        if (doorParticles != null && doorParticles.isPlaying)
        {
            doorParticles.Stop();
        }
    }

    // --- МЕТОД ДЛЯ РУЧНОГО ОТКРЫТИЯ (По кнопке E) ---
    public bool CanInteract()
    {
        // Кликнуть на дверь можно только если включен режим предметов, и она закрыта
        return useItemsToOpen && !isOpen && !isAnimating;
    }

    public void Interact()
    {
        if (!useItemsToOpen || isAnimating || isOpen) return;

        // Проверяем предметы у игрока
        if (CheckRequirements())
        {
            ConsumeRequiredItems();
            TargetOpen();
        }
        else
        {
            Debug.Log("У вас недостаточно предметов для открытия двери!");
        }
    }

    // --- МЕТОДЫ ДЛЯ ВНЕШНЕГО УПРАВЛЕНИЯ (Для скрипта кнопки PressurePlate) ---
    public void TargetOpen()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(AnimateDoor(openPos, openRot));
        isOpen = true;
    }

    public void TargetClose()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(AnimateDoor(closedPos, closedRot));
        isOpen = false;
    }

    // --- ВНУТРЕННЯЯ ЛОГИКА ---
    private bool CheckRequirements()
    {
        if (player == null) return false;

        List<GameObject> currentItems = player.GetFloatingObjectsList();
        List<string> neededIDs = new List<string>(requiredItemIDs);

        foreach (GameObject itemObj in currentItems)
        {
            if (itemObj == null) continue;

            GrabbableItem item = itemObj.GetComponent<GrabbableItem>();
            if (item != null && neededIDs.Contains(item.uniqueID))
            {
                neededIDs.Remove(item.uniqueID);
            }
        }

        return neededIDs.Count == 0;
    }

    private void ConsumeRequiredItems()
    {
        List<GameObject> currentItems = player.GetFloatingObjectsList();
        List<string> neededIDs = new List<string>(requiredItemIDs);

        for (int i = currentItems.Count - 1; i >= 0; i--)
        {
            GameObject itemObj = currentItems[i];
            if (itemObj == null) continue;

            GrabbableItem item = itemObj.GetComponent<GrabbableItem>();
            if (item != null && neededIDs.Contains(item.uniqueID))
            {
                neededIDs.Remove(item.uniqueID);
                currentItems.RemoveAt(i);
                Destroy(itemObj);
            }
        }
    }

    private IEnumerator AnimateDoor(Vector3 targetPos, Quaternion targetRot)
    {
        isAnimating = true;
        if (doorParticles != null) doorParticles.Play();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsedTime = 0f;

        // Рассчитываем время динамически на случай, если дверь открывали/закрывали на полпути
        float distance = Vector3.Distance(startPos, targetPos);
        float maxDistance = openUpwards ? moveAmount : rotationAngle;
        float currentOpenTime = maxDistance > 0 ? (distance / maxDistance) * openTime : openTime;

        if (currentOpenTime < 0.05f) currentOpenTime = 0.05f;

        while (elapsedTime < currentOpenTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / currentOpenTime);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        if (doorParticles != null) doorParticles.Stop();
        isAnimating = false;
    }
}
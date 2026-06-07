using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorOpener : MonoBehaviour, IInteractable
{
    [Header("Настройки открытия")]
    public List<string> requiredItemIDs; // ID предметов, которые откроют дверь
    public bool openUpwards = false;     // true - едет вверх, false - вращается как обычная дверь
    public float moveAmount = 2.5f;      // Расстояние сдвига (если openUpwards = true)
    public float rotationAngle = 90f;    // Угол поворота (если openUpwards = false)
    public float openTime = 1.5f;        // Время открытия в секундах

    private bool isOpen = false;
    private bool isAnimating = false;
    private PlayerGrabIso player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerGrabIso>();
    }

    public bool CanInteract() => !isOpen && !isAnimating;

    public void Interact()
    {
        if (isAnimating || isOpen) return;

        // Проверяем, есть ли у игрока все нужные предметы
        if (CheckRequirements())
        {
            // Сначала забираем и уничтожаем предметы
            ConsumeRequiredItems();
            // Затем плавно открываем дверь
            StartCoroutine(AnimateDoor());
        }
        else
        {
            Debug.Log("У вас недостаточно предметов для открытия двери!");
        }
    }

    private bool CheckRequirements()
    {
        if (player == null) return false;

        List<GameObject> currentItems = player.GetFloatingObjectsList();

        // Создаем копию списка требуемых ID, чтобы отмечать найденные
        List<string> neededIDs = new List<string>(requiredItemIDs);

        foreach (GameObject itemObj in currentItems)
        {
            if (itemObj == null) continue;

            GrabbableItem item = itemObj.GetComponent<GrabbableItem>();
            if (item != null && neededIDs.Contains(item.uniqueID))
            {
                neededIDs.Remove(item.uniqueID); // Удаляем из списка нужных, так как нашли его
            }
        }

        // Если список нужных пуст — значит нашли все ключи
        return neededIDs.Count == 0;
    }

    private void ConsumeRequiredItems()
    {
        List<GameObject> currentItems = player.GetFloatingObjectsList();
        List<string> neededIDs = new List<string>(requiredItemIDs);

        // Идем с конца списка, так как будем удалять элементы в процессе
        for (int i = currentItems.Count - 1; i >= 0; i--)
        {
            GameObject itemObj = currentItems[i];
            if (itemObj == null) continue;

            GrabbableItem item = itemObj.GetComponent<GrabbableItem>();
            if (item != null && neededIDs.Contains(item.uniqueID))
            {
                neededIDs.Remove(item.uniqueID);

                // Удаляем из списка хоровода игрока
                currentItems.RemoveAt(i);

                // Уничтожаем сам объект в сцене
                Destroy(itemObj);
            }
        }
    }

    private IEnumerator AnimateDoor()
    {
        isAnimating = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Вычисляем финальные точки назначения
        Vector3 targetPos = openUpwards ? startPos + Vector3.up * moveAmount : startPos;
        Quaternion targetRot = openUpwards ? startRot : startRot * Quaternion.Euler(0, rotationAngle, 0);

        float elapsedTime = 0f;

        while (elapsedTime < openTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / openTime);

            // Используем SmoothStep для красивого замедления в начале и конце движения
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (openUpwards)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            }

            yield return null; // Ждем следующий кадр
        }

        // Жестко фиксируем в финальной точке, чтобы избежать микро-погрешностей float
        if (openUpwards) transform.position = targetPos;
        else transform.rotation = targetRot;

        isOpen = true;
        isAnimating = false;
        Debug.Log("Дверь успешно открыта!");
    }
}
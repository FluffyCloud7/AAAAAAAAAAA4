using UnityEngine;
using System.Collections.Generic;

public class PlayerGrabIso : MonoBehaviour
{
    public Transform holdParent;
    public float grabRange = 2f;
    public KeyCode grabKey = KeyCode.E;

    [Header("Настройки летающих предметов")]
    public float floatHeight = 2.2f;       // Высота полета над игроком (уровень головы)
    public float floatRadius = 0.6f;       // Радиус орбиты хоровода вокруг головы
    public float followSpeed = 12f;        // Скорость следования за игроком (чуть поднял дефолт, чтобы не отставали)
    public float orbitSpeed = 120f;        // Скорость вращения предметов по орбите вокруг головы
    public float spacing = 0.5f;           // Оставлено для совместимости, в тригонометрии не используется

    // Слот для тяжелого предмета (в руках)
    private GameObject heldHeavyObject;
    private Rigidbody heldHeavyRb;
    private Collider heldHeavyCollider;

    // Список для легких предметов (над головой)
    private List<GameObject> floatingObjects = new List<GameObject>();

    private Collider playerCollider;
    private Animator animator;

    // Текущий накопительный угол для синхронного вращения всего хоровода
    private float currentOrbitAngle = 0f;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Кнопка E теперь отвечает ТОЛЬКО за поднятие/выбрасывание тяжелых коробок
        if (Input.GetKeyDown(grabKey))
        {
            if (HasInteractableNearby())
                return;

            if (heldHeavyObject == null)
            {
                TryGrabHeavy();
            }
            else
            {
                DropHeavy();
            }
        }

        // Логика удержания тяжелого предмета в руках
        if (heldHeavyObject != null)
            FollowHoldPoint();

        // Логика управления летающими над головой предметами
        if (floatingObjects.Count > 0)
            UpdateFloatingObjects();
    }

    // --- АВТОМАТИЧЕСКИЙ ПОДБОР ЛЕГКИХ ПРЕДМЕТОВ ПРИ СТОЛКНОВЕНИИ ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            GrabbableItem item = other.GetComponent<GrabbableItem>();

            // Если это легкий предмет — подбираем автоматически без всяких кнопок
            if (item != null && item.itemSize == ItemSize.LightFloating)
            {
                GameObject lightObj = other.gameObject;

                // Защита от повторного подбора
                if (floatingObjects.Contains(lightObj)) return;

                Rigidbody lightRb = lightObj.GetComponent<Rigidbody>();

                // Отключаем физику, чтобы он не падал и ни обо что не бился во время полета
                if (lightRb != null)
                {
                    lightRb.useGravity = false;
                    lightRb.isKinematic = true;
                }

                // Игнорируем коллизии с игроком на всякий случай
                Physics.IgnoreCollision(playerCollider, other, true);

                // Добавляем в облако над головой
                floatingObjects.Add(lightObj);

                Debug.Log($"Автоматически подобран легкий предмет: {item.itemName}");
            }
        }
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

    // Логика ручного поднятия коробки
    void TryGrabHeavy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Grabbable"))
            {
                GrabbableItem item = hit.GetComponent<GrabbableItem>();
                if (item == null || item.itemSize != ItemSize.HeavyInHands) continue;

                heldHeavyObject = hit.gameObject;
                heldHeavyRb = heldHeavyObject.GetComponent<Rigidbody>();
                heldHeavyCollider = hit;

                if (heldHeavyRb != null)
                {
                    heldHeavyRb.useGravity = false;
                    heldHeavyRb.isKinematic = true;
                }

                if (playerCollider != null && heldHeavyCollider != null)
                    Physics.IgnoreCollision(playerCollider, heldHeavyCollider, true);

                if (animator != null) animator.SetBool("IsHoldingHeavy", true);
                return;
            }
        }
    }

    void FollowHoldPoint()
    {
        heldHeavyObject.transform.position = holdParent.position;
        heldHeavyObject.transform.rotation = transform.rotation;
    }

    // --- МАГИЯ ОРБИТАЛЬНОГО ХОРОВОДА ---
    void UpdateFloatingObjects()
    {
        // Кружим базовый угол каждую секунду
        currentOrbitAngle += orbitSpeed * Time.deltaTime;
        if (currentOrbitAngle > 360f) currentOrbitAngle -= 360f;

        int count = floatingObjects.Count;
        if (count == 0) return;

        // Делим круг 360 градусов ровно на количество собранных предметов
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (floatingObjects[i] == null) continue;

            // Сдвигаем каждый следующий предмет на свой шаг по кругу
            float angleForThisItem = currentOrbitAngle + (i * angleStep);
            float radians = angleForThisItem * Mathf.Deg2Rad;

            // Вычисляем смещение на плоскости XZ относительно центра игрока
            float offsetX = Mathf.Cos(radians) * floatRadius;
            float offsetZ = Mathf.Sin(radians) * floatRadius;

            // Итоговая точка: центр игрока + высота + рассчитанный сдвиг по кругу
            Vector3 targetPos = transform.position + new Vector3(offsetX, floatHeight, offsetZ);

            // Мягко интерполируем позицию предмета к его точке на орбите
            floatingObjects[i].transform.position = Vector3.Lerp(
                floatingObjects[i].transform.position,
                targetPos,
                Time.deltaTime * followSpeed
            );

            // Заставляем саму модельку красиво вращаться вокруг собственного центра
            floatingObjects[i].transform.Rotate(Vector3.up, orbitSpeed * Time.deltaTime, Space.World);
        }
    }

    void DropHeavy()
    {
        if (heldHeavyRb != null)
        {
            heldHeavyRb.useGravity = true;
            heldHeavyRb.isKinematic = false;
        }

        if (playerCollider != null && heldHeavyCollider != null)
            Physics.IgnoreCollision(playerCollider, heldHeavyCollider, false);

        if (animator != null) animator.SetBool("IsHoldingHeavy", false);

        heldHeavyObject = null;
        heldHeavyRb = null;
        heldHeavyCollider = null;
    }

    public bool UseFloatingItem(string nameToUse)
    {
        for (int i = 0; i < floatingObjects.Count; i++)
        {
            GrabbableItem item = floatingObjects[i].GetComponent<GrabbableItem>();
            if (item != null && item.itemName == nameToUse)
            {
                GameObject obj = floatingObjects[i];
                floatingObjects.RemoveAt(i);
                Destroy(obj);
                return true;
            }
        }
        return false;
    }
}
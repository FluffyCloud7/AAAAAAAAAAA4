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
    public float followSpeed = 12f;        // Скорость следования за игроком
    public float orbitSpeed = 120f;        // Скорость вращения предметов по орбите вокруг головы
    public float spacing = 0.5f;           // Оставлено для совместимости

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
        // Кнопка E отвечает за диалоги/взаимодействия, а затем за коробки
        if (Input.GetKeyDown(grabKey))
        {
            if (TryInteractWithNearby())
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

        if (heldHeavyObject != null)
            FollowHoldPoint();

        if (floatingObjects.Count > 0)
            UpdateFloatingObjects();
    }

    // --- АВТОМАТИЧЕСКИЙ ПОДБОР ЛЕГКИХ ПРЕДМЕТОВ ПРИ СТОЛКНОВЕНИИ ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            GrabbableItem item = other.GetComponent<GrabbableItem>();

            if (item != null && item.itemSize == ItemSize.LightFloating)
            {
                GameObject lightObj = other.gameObject;

                if (floatingObjects.Contains(lightObj)) return;

                Rigidbody lightRb = lightObj.GetComponent<Rigidbody>();

                if (lightRb != null)
                {
                    lightRb.useGravity = false;
                    lightRb.isKinematic = true;
                }

                Physics.IgnoreCollision(playerCollider, other, true);
                floatingObjects.Add(lightObj);

                Debug.Log($"Автоматически подобран легкий предмет: {item.itemName}");
            }
        }
    }

    // --- ВЗАИМОДЕЙСТВИЕ С ИНТЕРАКТИВНЫМИ ОБЪЕКТАМИ (NPC) ---
    bool TryInteractWithNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact();
                return true;
            }
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
        currentOrbitAngle += orbitSpeed * Time.deltaTime;
        if (currentOrbitAngle > 360f) currentOrbitAngle -= 360f;

        int count = floatingObjects.Count;
        if (count == 0) return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (floatingObjects[i] == null) continue;

            float angleForThisItem = currentOrbitAngle + (i * angleStep);
            float radians = angleForThisItem * Mathf.Deg2Rad;

            float offsetX = Mathf.Cos(radians) * floatRadius;
            float offsetZ = Mathf.Sin(radians) * floatRadius;

            Vector3 targetPos = transform.position + new Vector3(offsetX, floatHeight, offsetZ);

            floatingObjects[i].transform.position = Vector3.Lerp(
                floatingObjects[i].transform.position,
                targetPos,
                Time.deltaTime * followSpeed
            );

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

    // --- МЕТОДЫ ДЛЯ ПЕРЕНОСА ПРЕДМЕТОВ МЕЖДУ СЦЕНАМИ ---

    // Отдает менеджеру текущую тяжелую коробку
    public GameObject GetHeavyObject()
    {
        return heldHeavyObject;
    }

    // Отдает менеджеру список летающих предметов
    public List<GameObject> GetFloatingObjectsList()
    {
        return floatingObjects;
    }

    // Принудительно возвращает предметы в логику игрока после загрузки сцены
    public void RestoreGrabbedItems(GameObject heavyObj, List<GameObject> targetsFloating)
    {
        // Возвращаем тяжелый предмет
        if (heavyObj != null)
        {
            heldHeavyObject = heavyObj;
            heldHeavyRb = heldHeavyObject.GetComponent<Rigidbody>();
            heldHeavyCollider = heldHeavyObject.GetComponent<Collider>();

            // На всякий случай обновляем игнорирование коллизий на новой сцене
            if (playerCollider != null && heldHeavyCollider != null)
                Physics.IgnoreCollision(playerCollider, heldHeavyCollider, true);

            if (animator != null) animator.SetBool("IsHoldingHeavy", true);
        }

        // Возвращаем хоровод
        floatingObjects.Clear();
        if (targetsFloating != null)
        {
            foreach (GameObject obj in targetsFloating)
            {
                if (obj != null)
                {
                    Collider col = obj.GetComponent<Collider>();
                    if (playerCollider != null && col != null)
                        Physics.IgnoreCollision(playerCollider, col, true);

                    floatingObjects.Add(obj);
                }
            }
        }
    }
}
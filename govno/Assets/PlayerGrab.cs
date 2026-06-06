using UnityEngine;
using System.Collections.Generic;

public class PlayerGrabIso : MonoBehaviour
{
    public Transform holdParent;
    public float grabRange = 2f;
    public KeyCode grabKey = KeyCode.E;

    [Header("Настройки летающих предметов")]
    public float floatHeight = 2.2f;
    public float floatRadius = 0.6f;
    public float followSpeed = 12f;
    public float orbitSpeed = 120f;
    public float spacing = 0.5f;

    private GameObject heldHeavyObject;
    private Rigidbody heldHeavyRb;
    private Collider heldHeavyCollider;

    private List<GameObject> floatingObjects = new List<GameObject>();

    private Collider playerCollider;
    private Animator animator;

    private float currentOrbitAngle = 0f;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (TryInteractWithNearby()) return;

            if (heldHeavyObject == null) TryGrabHeavy();
            else DropHeavy();
        }

        if (heldHeavyObject != null) FollowHoldPoint();
        if (floatingObjects.Count > 0) UpdateFloatingObjects();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            GrabbableItem item = other.GetComponent<GrabbableItem>();

            if (item != null && item.itemSize == ItemSize.LightFloating)
            {
                item.SetPickedUp(true); // Отключаем покачивание

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
            }
        }
    }

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

    void TryGrabHeavy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Grabbable"))
            {
                GrabbableItem item = hit.GetComponent<GrabbableItem>();
                if (item == null || item.itemSize != ItemSize.HeavyInHands) continue;

                item.SetPickedUp(true); // Отключаем покачивание

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
        if (heldHeavyObject != null)
        {
            GrabbableItem item = heldHeavyObject.GetComponent<GrabbableItem>();
            if (item != null) item.SetPickedUp(false); // Включаем покачивание снова

            if (heldHeavyRb != null)
            {
                heldHeavyRb.useGravity = true;
                heldHeavyRb.isKinematic = false;
            }

            if (playerCollider != null && heldHeavyCollider != null)
                Physics.IgnoreCollision(playerCollider, heldHeavyCollider, false);

            if (animator != null) animator.SetBool("IsHoldingHeavy", false);
        }

        heldHeavyObject = null;
        heldHeavyRb = null;
        heldHeavyCollider = null;
    }

    // ВОЗВРАЩЕНЫ НУЖНЫЕ МЕТОДЫ:
    public GameObject GetHeavyObject() => heldHeavyObject;
    public List<GameObject> GetFloatingObjectsList() => floatingObjects;

    public void RestoreGrabbedItems(GameObject heavyObj, List<GameObject> targetsFloating)
    {
        if (heavyObj != null)
        {
            heldHeavyObject = heavyObj;
            heldHeavyRb = heldHeavyObject.GetComponent<Rigidbody>();
            heldHeavyCollider = heldHeavyObject.GetComponent<Collider>();
            if (playerCollider != null && heldHeavyCollider != null)
                Physics.IgnoreCollision(playerCollider, heldHeavyCollider, true);
            if (animator != null) animator.SetBool("IsHoldingHeavy", true);
        }

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
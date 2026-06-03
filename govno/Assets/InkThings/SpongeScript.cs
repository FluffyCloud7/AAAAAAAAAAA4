using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    [Header("Настройки луча и стирания")]
    public float eraseRadius = 0.15f;
    public float rayDistance = 4f;

    [Header("Емкость губки")]
    public float maxInkCapacity = 100f;
    public float absorptionRate = 0.5f;
    private float currentInkAmount = 0f;

    [Header("Текстуры для Shader Graph")]
    [Tooltip("Твоя чистая желтая текстура")]
    public Texture2D cleanSpongeTexture;
    [Tooltip("Твоя грязная фиолетовая текстура")]
    public Texture2D dirtySpongeTexture;

    [Header("Визуализация")]
    public MeshRenderer spongeRenderer;
    private Material spongeMaterial;

    // Переменные для отслеживания движения губки
    private Vector3 lastPosition;
    private float movementThreshold = 0.01f;

    void Start()
    {
        if (spongeRenderer == null)
        {
            spongeRenderer = GetComponent<MeshRenderer>();
        }

        if (spongeRenderer != null)
        {
            spongeMaterial = spongeRenderer.material;

            // Передаем текстуры в Shader Graph по именам из твоего Blackboard
            if (cleanSpongeTexture != null && spongeMaterial.HasProperty("_BaseTexture"))
            {
                spongeMaterial.SetTexture("_BaseTexture", cleanSpongeTexture);
            }
            if (dirtySpongeTexture != null && spongeMaterial.HasProperty("_DirtyTexture"))
            {
                spongeMaterial.SetTexture("_DirtyTexture", dirtySpongeTexture);
            }

            UpdateSpongeVisual();
        }

        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // Если губка заполнена — стоп работа
        if (currentInkAmount >= maxInkCapacity)
        {
            Debug.DrawRay(transform.position + Vector3.up * 1.0f, Vector3.down * rayDistance, Color.gray);
            return;
        }

        Vector3 downDirection = Vector3.down;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
        int inkLayerMask = LayerMask.GetMask("InkPuddle");

        if (inkLayerMask == 0)
        {
            inkLayerMask = ~LayerMask.GetMask("Ignore Raycast");
        }

        RaycastHit hit;

        // ТВОЙ ОРИГИНАЛЬНЫЙ ЛУЧ
        if (Physics.Raycast(rayOrigin, downDirection, out hit, rayDistance, inkLayerMask, QueryTriggerInteraction.Ignore))
        {
            // Ищем оригинальный GPU скрипт лужи
            InkSystem ink = hit.collider.GetComponent<InkSystem>();
            if (ink != null)
            {
                // Вызываем стирание напрямую по твоей логике
                ink.Erase(hit.textureCoord, eraseRadius);

                // Проверяем движение губки
                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                if (distanceMoved > movementThreshold)
                {
                    // Пачкается только при движении по грязи
                    currentInkAmount += absorptionRate;
                    currentInkAmount = Mathf.Clamp(currentInkAmount, 0f, maxInkCapacity);
                    UpdateSpongeVisual();
                }
            }
        }

        lastPosition = transform.position;
        Debug.DrawRay(rayOrigin, downDirection * rayDistance, Color.red);
    }

    private void UpdateSpongeVisual()
    {
        if (spongeMaterial != null)
        {
            float progress = currentInkAmount / maxInkCapacity;

            // Крутим ползунок прогресса в Shader Graph
            if (spongeMaterial.HasProperty("_SpongeDirtProgress"))
            {
                spongeMaterial.SetFloat("_SpongeDirtProgress", progress);
            }
        }
    }

    public void WashSponge()
    {
        currentInkAmount = 0f;
        UpdateSpongeVisual();
    }

    private void OnMouseEnter()
    {
        if (CursorVisualController.Instance != null) CursorVisualController.Instance.SetInteractableState(true);
    }

    private void OnMouseExit()
    {
        if (CursorVisualController.Instance != null) CursorVisualController.Instance.SetInteractableState(false);
    }

    private void OnDisable()
    {
        if (CursorVisualController.Instance != null) CursorVisualController.Instance.SetInteractableState(false);
    }
}
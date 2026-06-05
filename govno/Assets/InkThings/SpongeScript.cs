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

        if (Physics.Raycast(rayOrigin, downDirection, out hit, rayDistance, inkLayerMask, QueryTriggerInteraction.Ignore))
        {
            InkSystem ink = hit.collider.GetComponent<InkSystem>();
            if (ink != null)
            {
                ink.Erase(hit.textureCoord, eraseRadius);

                float distanceMoved = Vector3.Distance(transform.position, lastPosition);
                if (distanceMoved > movementThreshold)
                {
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

            if (spongeMaterial.HasProperty("_SpongeDirtProgress"))
            {
                spongeMaterial.SetFloat("_SpongeDirtProgress", progress);
            }
        }
    }

    // МЕТОД ОЧИЩЕНИЯ: Вызывается скриптом воды при входе в триггер
    public void WashSponge()
    {
        currentInkAmount = 0f; // Сбрасываем счетчик грязи в ноль
        UpdateSpongeVisual();  // Возвращаем шейдер в чистое желтое состояние
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
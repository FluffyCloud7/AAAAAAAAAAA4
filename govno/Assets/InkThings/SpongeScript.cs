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

    [Header("Текстуры губки")]
    [Tooltip("Чистая желтая текстура")]
    public Texture2D cleanSpongeTexture;
    [Tooltip("Грязная фиолетовая текстура из Фотошопа")]
    public Texture2D dirtySpongeTexture;

    [Header("Визуализация")]
    public MeshRenderer spongeRenderer;

    private Material spongeMaterial;

    void Start()
    {
        if (spongeRenderer == null)
        {
            spongeRenderer = GetComponent<MeshRenderer>();
        }

        if (spongeRenderer != null)
        {
            spongeMaterial = spongeRenderer.material;

            // Передаем текстуры в Shader Graph при старте
            if (cleanSpongeTexture != null && spongeMaterial.HasProperty("_BaseTexture"))
            {
                spongeMaterial.SetTexture("_BaseTexture", cleanSpongeTexture);
            }

            // Загружаем фиолетовую грязную текстуру в новое свойство
            if (dirtySpongeTexture != null && spongeMaterial.HasProperty("_DirtyTexture"))
            {
                spongeMaterial.SetTexture("_DirtyTexture", dirtySpongeTexture);
            }

            UpdateSpongeVisual();
        }
        else
        {
            Debug.LogWarning("[SpongeCleaner]: Не найден MeshRenderer на объекте!");
        }
    }

    void FixedUpdate()
    {
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

                currentInkAmount += absorptionRate;
                currentInkAmount = Mathf.Clamp(currentInkAmount, 0f, maxInkCapacity);

                UpdateSpongeVisual();
            }
        }

        Debug.DrawRay(rayOrigin, downDirection * rayDistance, Color.red);
    }

    private void UpdateSpongeVisual()
    {
        if (spongeMaterial != null)
        {
            // Рассчитываем прогресс от 0.0 до 1.0
            float progress = currentInkAmount / maxInkCapacity;

            // Двигаем твой ползунок SpongeDirtProgress внутри Shader Graph
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
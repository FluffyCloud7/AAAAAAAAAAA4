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

    // Авто-чекпоинт позиции (чтобы не бегать за ней в начало уровня)
    private Vector3 safeSpawnPosition;
    private Quaternion safeSpawnRotation;
    private Rigidbody rb;
    private float checkpointTimer = 0f;

    void Start()
    {
        // По умолчанию стартовая точка — безопасная
        safeSpawnPosition = transform.position;
        safeSpawnRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();

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
        // Обновляем точку сохранения, если губка спокойно лежит на твердой поверхности и не двигается
        UpdateSafeCheckpoint();

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

    // Логика умного чекпоинта
    private void UpdateSafeCheckpoint()
    {
        if (rb == null) return;

        // Если губка почти не двигается (скорость близка к нулю)
        if (rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            checkpointTimer += Time.fixedDeltaTime;

            // Если она стабильно лежит дольше 0.5 секунд, пускаем короткий луч вниз проверки земли
            if (checkpointTimer > 0.5f)
            {
                // Проверяем, что под нами твердый пол, а не триггер бездны
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f, ~LayerMask.GetMask("InkPuddle"), QueryTriggerInteraction.Ignore))
                {
                    // Сохраняем это место как безопасное
                    safeSpawnPosition = transform.position + Vector3.up * 0.1f; // чуть приподнимем, чтоб не застревала в полу
                    safeSpawnRotation = transform.rotation;
                }
                checkpointTimer = 0f;
            }
        }
        else
        {
            // Если губка летит или игрок её тащит — сбрасываем таймер
            checkpointTimer = 0f;
        }
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

    public void WashSponge()
    {
        currentInkAmount = 0f;
        UpdateSpongeVisual();
    }

    // МЕТОД РЕСПАВНА: Возвращает конкретную губку на её ПОСЛЕДНЕЕ безопасное место
    public void RespawnSponge()
    {
        transform.position = safeSpawnPosition;
        transform.rotation = safeSpawnRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        WashSponge();
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
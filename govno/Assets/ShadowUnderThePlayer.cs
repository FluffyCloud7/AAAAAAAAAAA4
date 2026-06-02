using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    public enum ShaderColorProperty
    {
        _Color,      // Стандартный Built-in шейдер
        _BaseColor,  // Стандартный URP (Universal RP) шейдер
        _MainColor   // Некоторые кастомные/мобильные шейдеры
    }

    [Header("References")]
    public Transform player;

    [Header("Color Settings")]
    [Tooltip("Тип свойства цвета в вашем шейдере. Если цвет не меняется, переключите на _BaseColor (актуально для URP)")]
    public ShaderColorProperty colorProperty = ShaderColorProperty._Color;

    [Tooltip("Цвет тени. Настраивать цвет и альфу нужно ЗДЕСЬ.")]
    [SerializeField] private Color shadowColor = new Color(0.0f, 0.1f, 0.3f);

    [Header("Distance & Fade")]
    public float maxSearchDistance = 20f;
    public float fadeDistance = 4f;

    [Header("Scale")]
    public float maxScale = 1.5f;
    public float minScale = 0.25f;

    [Header("Opacity")]
    [Range(0f, 1f)] public float maxAlpha = 0.6f;
    [Range(0f, 1f)] public float minAlpha = 0.0f;

    [Header("Position & Tweaks")]
    public float groundOffset = 0.04f;
    public float raycastOriginOffset = 0.5f;

    [Header("Rotation Setup")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(90f, 0f, 0f);

    private Renderer rend;
    private MaterialPropertyBlock propBlock;
    private int activeColorId;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Кешируем выбранное имя свойства цвета
        activeColorId = Shader.PropertyToID(colorProperty.ToString());
    }

    // Автоматически обновляем ID свойства, если вы поменяли его в инспекторе во время игры
    void OnValidate()
    {
        activeColorId = Shader.PropertyToID(colorProperty.ToString());
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 rayOrigin = player.position + Vector3.up * raycastOriginOffset;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, maxSearchDistance + raycastOriginOffset);

        RaycastHit bestHit = new RaycastHit();
        bool foundValidGround = false;
        float highestY = -Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            if (hit.transform == player || hit.transform.IsChildOf(player)) continue;

            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
                bestHit = hit;
                foundValidGround = true;
            }
        }

        if (foundValidGround)
        {
            if (!rend.enabled) rend.enabled = true;

            Vector3 shadowPos = new Vector3(player.position.x, bestHit.point.y + groundOffset, player.position.z);
            transform.position = shadowPos;
            transform.rotation = Quaternion.Euler(fixedRotation);

            float distance = player.position.y - bestHit.point.y;
            float t = Mathf.Clamp01(distance / fadeDistance);

            // МАСШТАБ
            float scale = Mathf.Lerp(maxScale, minScale, t);
            transform.localScale = new Vector3(scale, scale, scale);

            // ЦВЕТ И АЛЬФА (через MaterialPropertyBlock)
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);
            Color finalColor = shadowColor;
            finalColor.a = alpha;

            // Читаем текущие свойства, меняем цвет, записываем обратно
            rend.GetPropertyBlock(propBlock);
            propBlock.SetColor(activeColorId, finalColor);
            rend.SetPropertyBlock(propBlock);
        }
        else
        {
            if (rend.enabled) rend.enabled = false;
        }
    }
}
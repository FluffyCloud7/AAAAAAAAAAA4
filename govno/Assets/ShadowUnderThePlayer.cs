using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Color Settings")]
    [Tooltip("Цвет тени (выберите здесь тёмно-синий)")]
    [SerializeField] private Color shadowColor = new Color(0.0f, 0.1f, 0.3f);

    [Header("Distance")]
    public float maxDistance = 6f;

    [Header("Scale")]
    [Tooltip("Размер тени, когда игрок стоит на земле")]
    public float maxScale = 1.5f;
    [Tooltip("Размер тени на максимальной высоте")]
    public float minScale = 0.25f;

    [Header("Opacity")]
    [Range(0f, 1f)] public float maxAlpha = 0.6f;
    [Range(0f, 1f)] public float minAlpha = 0.2f;

    [Header("Position & Alignment")]
    [Tooltip("Смещение над землей во избежание Z-fighting")]
    public float groundOffset = 0.02f;
    [Tooltip("Высота, на которую приподнимается точка пуска луча относительно игрока")]
    public float raycastOriginOffset = 0.5f;

    private Renderer rend;
    private Material mat;
    // Кэшируем ID свойства цвета для оптимизации работы с материалом
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    void Start()
    {
        rend = GetComponent<Renderer>();

        // Используем вызов для создания уникального экземпляра материала
        mat = rend.material;
    }

    void LateUpdate()
    {
        if (player == null) return;

        RaycastHit hit;
        // Пускаем луч чуть выше ног игрока, чтобы он не застревал в мелких порожках
        Vector3 rayOrigin = player.position + Vector3.up * raycastOriginOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, maxDistance + raycastOriginOffset))
        {
            if (!rend.enabled) rend.enabled = true;

            // ===== POSITION =====
            // Сдвигаем позицию тени немного вдоль нормали поверхности, а не просто строго вверх
            Vector3 shadowPos = hit.point + hit.normal * groundOffset;
            transform.position = shadowPos;

            // ===== ROTATION (Выравнивание по геометрии) =====
            // Заставляем верх (transform.up) объекта тени смотреть туда же, куда смотрит нормаль земли
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // ===== DISTANCE =====
            // Расстояние считаем честно от ног игрока до точки столкновения
            float distance = player.position.y - hit.point.y;
            float t = Mathf.Clamp01(distance / maxDistance);

            // ===== SCALE =====
            float scale = Mathf.Lerp(maxScale, minScale, t);
            transform.localScale = new Vector3(scale, scale, scale);

            // ===== COLOR & OPACITY =====
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);

            // Формируем финальный цвет, комбинируя выбранный синий и посчитанную альфу
            Color finalColor = shadowColor;
            finalColor.a = alpha;

            // Меняем цвет через SetColor (работает со стандартными шейдерами и URP)
            mat.SetColor(ColorPropertyId, finalColor);
        }
        else
        {
            if (rend.enabled) rend.enabled = false;
        }
    }

    private void OnDestroy()
    {
        // На всякий случай чистим за собой материал при уничтожении объекта
        if (mat != null) Destroy(mat);
    }
}
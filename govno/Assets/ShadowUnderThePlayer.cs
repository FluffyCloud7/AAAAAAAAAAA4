using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Distance")]
    public float maxDistance = 6f;

    [Header("Scale")]
    [Tooltip("Размер тени, когда игрок стоит на земле")]
    public float maxScale = 1.5f; // <-- НАСТРОЙКА: Сделайте больше, например 1.5 или 2.0
    [Tooltip("Размер тени на максимальной высоте")]
    public float minScale = 0.25f;

    [Header("Opacity")]
    public float maxAlpha = 0.6f;
    public float minAlpha = 0.2f;

    [Header("Position")]
    public float groundOffset = 0.02f;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    void LateUpdate()
    {
        // Проверка на null, чтобы избежать ошибок в консоли, если плеер не назначен
        if (player == null) return;

        RaycastHit hit;

        if (Physics.Raycast(player.position, Vector3.down, out hit, maxDistance))
        {
            // Включаем тень, если она была выключена
            if (!rend.enabled) rend.enabled = true;

            // ===== POSITION =====
            Vector3 shadowPos = hit.point;
            shadowPos.y += groundOffset;
            transform.position = shadowPos;

            // ===== DISTANCE =====
            float distance = player.position.y - hit.point.y;
            float t = Mathf.Clamp01(distance / maxDistance);

            // ===== SCALE =====
            // Теперь вместо 1f используется настраиваемый maxScale
            float scale = Mathf.Lerp(maxScale, minScale, t);
            transform.localScale = new Vector3(scale, scale, scale);

            // ===== OPACITY =====
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
        else
        {
            // Если луч не попал в землю (игрок слишком высоко), скрываем тень
            if (rend.enabled) rend.enabled = false;
        }
    }
}
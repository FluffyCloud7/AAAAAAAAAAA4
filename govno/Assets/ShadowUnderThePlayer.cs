using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Distance")]
    public float maxDistance = 6f;

    [Header("Scale")]
    public float minScale = 0.25f;

    [Header("Opacity")]
    public float maxAlpha = 0.6f; // на земле
    public float minAlpha = 0.2f; // в воздухе

    [Header("Position")]
    public float groundOffset = 0.02f;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // —оздаЄм отдельный экземпл€р материала
        mat = rend.material;
    }

    void LateUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(player.position, Vector3.down, out hit, maxDistance))
        {
            // ===== POSITION =====

            Vector3 shadowPos = hit.point;
            shadowPos.y += groundOffset;

            transform.position = shadowPos;

            // ===== DISTANCE =====

            float distance = player.position.y - hit.point.y;
            float t = Mathf.Clamp01(distance / maxDistance);

            // ===== SCALE =====

            float scale = Mathf.Lerp(1f, minScale, t);

            transform.localScale = new Vector3(scale, scale, scale);

            // ===== OPACITY =====

            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);

            Color c = mat.color;
            c.a = alpha;

            mat.color = c;
        }
    }
}
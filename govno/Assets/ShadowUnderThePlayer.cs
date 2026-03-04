using UnityEngine;

public class SimpleShadow : MonoBehaviour
{
    public Transform player;
    public float maxDistance = 6f;
    public float minScale = 0.25f;   // насколько сильно сжимается

    void LateUpdate()
    {
        RaycastHit hit;

        if (Physics.Raycast(player.position, Vector3.down, out hit, maxDistance))
        {
            Vector3 shadowPos = hit.point;
            shadowPos.y += 0.02f;

            transform.position = shadowPos;

            float distance = player.position.y - hit.point.y;
            float t = Mathf.Clamp01(distance / maxDistance);

            float scale = Mathf.Lerp(1f, minScale, t);

            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
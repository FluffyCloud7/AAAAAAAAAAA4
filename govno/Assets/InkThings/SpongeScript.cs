using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    public float eraseRadius = 0.15f;
    public float rayDistance = 2f;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            InkSystem puddle = hit.collider.GetComponent<InkSystem>();

            if (puddle != null)
                puddle.Erase(hit.textureCoord, eraseRadius);
        }
    }
}
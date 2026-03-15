using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    public float eraseRadius = 0.15f;
    public float rayDistance = 2f;

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.red);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            InkPuddleMask puddle = hit.collider.GetComponent<InkPuddleMask>();

            if (puddle != null)
                puddle.Erase(hit.textureCoord, eraseRadius);
        }
    }
}
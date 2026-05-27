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

    // СРАБАТЫВАЕТ, КОГДА КУРСОР МЫШИ НАВЕДЕН НА ГУБКУ
    private void OnMouseEnter()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(true); // Курсор становится фиолетовым!
        }
    }

    // СРАБАТЫВАЕТ, КОГДА КУРСОР УХОДИТ С ГУБКИ
    private void OnMouseExit()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(false); // Курсор возвращается в белый
        }
    }

    // На случай, если губку выключат или уничтожат прямо под мышью
    private void OnDisable()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(false);
        }
    }
}
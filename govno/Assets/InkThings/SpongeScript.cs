using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    public float eraseRadius = 0.15f;
    [Tooltip("Длина луча вниз. Сделай её около 3-5, чтобы губка терла только когда касается пола.")]
    public float rayDistance = 4f;

    void FixedUpdate()
    {
        // Направление всегда строго вниз к луже (вдоль мировой оси Y)
        Vector3 downDirection = Vector3.down;

        // Начинаем луч на 1 единицу ВЫШЕ центра губки. 
        // Это гарантирует, что луч прошьет нижнюю грань губки насквозь и найдет пол,
        // даже если губка сильно прижата камерой или наклонилась.
        Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;

        // Ищем твой существующий слой InkPuddle
        int inkLayerMask = LayerMask.GetMask("InkPuddle");

        // Если вдруг опечатались в имени, маска станет 0. Сделаем бэкап, чтобы не ломать игру
        if (inkLayerMask == 0)
        {
            inkLayerMask = ~LayerMask.GetMask("Ignore Raycast");
        }

        RaycastHit hit;

        // Стреляем строго вниз по слою InkPuddle, игнорируя триггеры урона
        if (Physics.Raycast(rayOrigin, downDirection, out hit, rayDistance, inkLayerMask, QueryTriggerInteraction.Ignore))
        {
            InkSystem ink = hit.collider.GetComponent<InkSystem>();
            if (ink != null)
            {
                // Стираем в точных текстурных координатах касания
                ink.Erase(hit.textureCoord, eraseRadius);
            }
        }

        // РИСУЕМ ЛУЧ В ОКНЕ СЦЕНЫ (Для отладки)
        // Красный луч покажет, откуда и куда бьет проверка стирания
        Debug.DrawRay(rayOrigin, downDirection * rayDistance, Color.red);
    }

    private void OnMouseEnter()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(true);
        }
    }

    private void OnMouseExit()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(false);
        }
    }

    private void OnDisable()
    {
        if (CursorVisualController.Instance != null)
        {
            CursorVisualController.Instance.SetInteractableState(false);
        }
    }
}
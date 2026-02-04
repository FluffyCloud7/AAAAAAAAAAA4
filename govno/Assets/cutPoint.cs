using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CutPoint : MonoBehaviour
{
    private CuttableShape parent;
    public bool Visited { get; private set; }

    public void Init(CuttableShape shape)
    {
        parent = shape;
        Visited = false;
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(0)) return;

        // УБИРАЕМ ЗДЕСЬ if (Visited) return;
        // Теперь точка всегда сообщает о касании, а CuttableShape фильтрует
        CutManager.Instance.VisitPoint(this);
    }

    public void MarkVisited()
    {
        Visited = true;
        if (TryGetComponent<Renderer>(out var r))
            r.material.color = Color.green;
    }

    public void ResetPoint()
    {
        Visited = false;
        // Возвращаем исходный цвет (например, белый)
        if (TryGetComponent<Renderer>(out var r))
            r.material.color = Color.white;
    }
}
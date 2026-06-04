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

        // Если менеджер почему-то потерял форму, принудительно напоминаем ему о ней
        if (parent != null)
        {
            CutManager.Instance.StartCut(parent);
        }

        // Отправляем данные в менеджер, как и было изначально
        CutManager.Instance.VisitPoint(this);
    }

    public void MarkVisited()
    {
        Visited = true;
        // Ищет Renderer в том числе на ваших дочерних кастомных 3D-моделях точек
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.material.color = Color.green; // Или любая ваша логика подсветки
        }
    }

    public void ResetPoint()
    {
        Visited = false;
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.material.color = Color.white;
        }
    }
}
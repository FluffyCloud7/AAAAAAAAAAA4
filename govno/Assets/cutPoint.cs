using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Или Collider, если у тебя 3D-коллайдеры
public class CutPoint : MonoBehaviour
{
    public enum PointType { Core, Flap }

    [Header("Тип точки для логики")]
    public PointType pointType = PointType.Core;

    private CuttableShape parent;
    public bool Visited { get; private set; }

    [Header("Спрайты точки")]
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite visitedSprite;

    private SpriteRenderer spriteRenderer;

    public void Init(CuttableShape shape)
    {
        parent = shape;
        Visited = false;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        SetPointSprite(neutralSprite);
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(0)) return;
        if (parent != null) CutManager.Instance.StartCut(parent);
        CutManager.Instance.VisitPoint(this);
    }

    public void MarkVisited()
    {
        if (Visited) return;
        Visited = true;
        SetPointSprite(visitedSprite);
    }

    public void ResetPoint()
    {
        Visited = false;
        SetPointSprite(neutralSprite);
    }

    private void SetPointSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null) spriteRenderer.sprite = newSprite;
    }
}
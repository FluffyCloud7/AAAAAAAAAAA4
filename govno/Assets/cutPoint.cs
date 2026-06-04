using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CutPoint : MonoBehaviour
{
    public enum PointType { Core, Flap } // Core - грань фигуры, Flap - ушко

    private CuttableShape parent;
    public bool Visited { get; private set; }

    [Header("Тип точки для логики")]
    public PointType pointType;

    [Header("Спрайты точки")]
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite visitedSprite;

    private SpriteRenderer spriteRenderer;

    public void Init(CuttableShape shape)
    {
        parent = shape;
        Visited = false;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogError($"На точке {gameObject.name} нет SpriteRenderer!");

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
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    public void ResetPoint()
    {
        Visited = false;
        SetPointSprite(neutralSprite);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    private void SetPointSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null) spriteRenderer.sprite = newSprite;
    }
}
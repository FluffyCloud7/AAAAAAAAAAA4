using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CutPoint : MonoBehaviour
{
    private CuttableShape parent;
    public bool Visited { get; private set; }

    [Header("Спрайты точки (2D Картинки)")]
    [Tooltip("Нейтральный спрайт, когда точку еще не трогали")]
    [SerializeField] private Sprite neutralSprite;

    [Tooltip("Обведенный спрайт, когда через точку провели линию")]
    [SerializeField] private Sprite visitedSprite;

    private SpriteRenderer spriteRenderer;

    public void Init(CuttableShape shape)
    {
        parent = shape;
        Visited = false;

        // Ищем SpriteRenderer на самой точке или в её детях
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"Внимание! На точке {gameObject.name} не найден компонент SpriteRenderer! Добавь его.");
        }

        // Ставим дефолтный нейтральный спрайт
        SetPointSprite(neutralSprite);
    }

    private void OnMouseOver()
    {
        if (!Input.GetMouseButton(0)) return;

        if (parent != null)
        {
            CutManager.Instance.StartCut(parent);
        }

        CutManager.Instance.VisitPoint(this);
    }

    public void MarkVisited()
    {
        if (Visited) return;
        Visited = true;

        // Меняем картинку на обведенную
        SetPointSprite(visitedSprite);

        // На всякий случай сбрасываем старый зеленый цвет в дефолтный белый,
        // чтобы он не накладывался поверх твоей новой текстуры
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void ResetPoint()
    {
        Visited = false;

        // Возвращаем нейтральный спрайт
        SetPointSprite(neutralSprite);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void SetPointSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
}
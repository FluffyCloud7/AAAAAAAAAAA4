using UnityEngine;

public class CutManager : MonoBehaviour
{
    public static CutManager Instance;
    private CuttableShape currentShape;

    private void Awake() => Instance = this;

    public void StartCut(CuttableShape shape)
    {
        currentShape = shape;
    }

    private void Update()
    {
        // Если кнопка отпущена — сбрасываем прогресс текущей формы
        if (Input.GetMouseButtonUp(0))
        {
            StopCut();
        }
    }

    public void StopCut()
    {
        if (currentShape != null)
        {
            currentShape.ResetProgress();
        }
        currentShape = null;
    }

    public void VisitPoint(CutPoint point)
    {
        if (currentShape == null) return;

        if (Input.GetMouseButton(0))
        {
            currentShape.VisitPoint(point);
        }
    }
}
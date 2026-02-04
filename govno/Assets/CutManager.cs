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
        // Самый важный момент: если кнопка отпущена — сбрасываем всё
        if (Input.GetMouseButtonUp(0))
        {
            StopCut();
        }
    }

    public void StopCut()
    {
        if (currentShape != null)
        {
            currentShape.ResetProgress(); // Вызываем полный сброс
        }
        currentShape = null;
    }

    public void VisitPoint(CutPoint point)
    {
        if (currentShape == null) return;
        // Передаем касание, только если кнопка зажата
        if (Input.GetMouseButton(0))
        {
            currentShape.VisitPoint(point);
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class CuttableShape : MonoBehaviour
{
    public List<CutPoint> points;
    public GameObject resultPrefab;
    public Transform spawnPoint;

    private int visitedCount = 0;
    private bool completed = false;
    private int lastVisitedIndex = -1;
    private int firstVisitedIndex = -1;
    private int direction = 0;

    private LineRenderer lineRenderer;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        foreach (var point in points) point.Init(this);
    }

    private void Update()
    {
        // Рисуем хвост линии, только если мы начали резать и кнопка зажата
        if (!completed && lastVisitedIndex != -1 && Input.GetMouseButton(0))
        {
            UpdateTrailingLine();
        }
    }

    private void UpdateTrailingLine()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

        lineRenderer.positionCount = visitedCount + 1;
        lineRenderer.SetPosition(visitedCount, worldMousePos);
    }

    // Сообщаем менеджеру о форме при наведении
    private void OnMouseEnter()
    {
        if (!completed) CutManager.Instance.StartCut(this);
    }

    public void VisitPoint(CutPoint point)
    {
        if (completed) return;
        int currentIndex = points.IndexOf(point);
        if (currentIndex == lastVisitedIndex) return;

        if (lastVisitedIndex == -1)
        {
            firstVisitedIndex = currentIndex;
            AcceptPoint(point, currentIndex);
            return;
        }

        if (visitedCount == points.Count && currentIndex == firstVisitedIndex)
        {
            if (IsCorrectNext(currentIndex)) CloseAndFinish(point);
            return;
        }

        if (point.Visited) return;

        if (direction == 0)
        {
            if (IsNeighbor(currentIndex, lastVisitedIndex, out int detectedDir))
            {
                direction = detectedDir;
                AcceptPoint(point, currentIndex);
            }
        }
        else if (IsCorrectNext(currentIndex))
        {
            AcceptPoint(point, currentIndex);
        }
    }

    private bool IsNeighbor(int current, int last, out int dir)
    {
        int total = points.Count;
        dir = 0;
        if (current == (last + 1) % total) { dir = 1; return true; }
        if (current == (last - 1 + total) % total) { dir = -1; return true; }
        return false;
    }

    private bool IsCorrectNext(int current)
    {
        int total = points.Count;
        int expectedIndex = (lastVisitedIndex + direction + total) % total;
        return current == expectedIndex;
    }

    private void AcceptPoint(CutPoint point, int index)
    {
        point.MarkVisited();
        visitedCount++;
        lastVisitedIndex = index;

        lineRenderer.positionCount = visitedCount;
        lineRenderer.SetPosition(visitedCount - 1, point.transform.position);
    }

    // ПОЛНЫЙ СБРОС (вызывается из менеджера при отжатии кнопки)
    public void ResetProgress()
    {
        if (completed) return;

        visitedCount = 0;
        lastVisitedIndex = -1;
        firstVisitedIndex = -1;
        direction = 0;

        foreach (var point in points)
        {
            point.ResetPoint(); // Убедись, что этот метод есть в CutPoint
        }

        if (lineRenderer != null) lineRenderer.positionCount = 0;

        Debug.Log("Прогресс сброшен: кнопка отпущена.");
    }

    private void CloseAndFinish(CutPoint startPoint)
    {
        completed = true;
        lineRenderer.positionCount = visitedCount + 1;
        lineRenderer.SetPosition(visitedCount, startPoint.transform.position);

        Instantiate(resultPrefab, spawnPoint != null ? spawnPoint.position : transform.position + Vector3.right * 2f, Quaternion.identity);
        Destroy(gameObject);
    }
}
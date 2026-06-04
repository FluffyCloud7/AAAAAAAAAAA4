using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class CuttableShape : MonoBehaviour
{
    public List<CutPoint> points;
    public GameObject resultPrefab;
    public Transform spawnPoint;

    [Header("Настройки анимации")]
    [Tooltip("Материал со сплошной красивой текстурой мазка (Texture Mode: Stretch)")]
    [SerializeField] private Material solidLineMaterial;

    [Tooltip("Время анимации разреза в секундах")]
    [SerializeField] private float cutAnimationDuration = 0.8f;

    [Tooltip("Множитель толщины для финальной линии (если она слишком толстая, поставьте например 0.3)")]
    [Range(0.05f, 2f)]
    [SerializeField] private float solidLineWidthMultiplier = 0.3f;

    [Tooltip("Смещение линии вперед, чтобы она не сливалась с фоном (попробуйте от -0.05 до -0.2)")]
    [SerializeField] private float zOffset = -0.1f;

    private int visitedCount = 0;
    private bool completed = false;
    private int lastVisitedIndex = -1;
    private int firstVisitedIndex = -1;
    private int direction = 0;

    private LineRenderer dottedLineRenderer; // Пунктир (основной)
    private LineRenderer solidLineRenderer;  // Сплошной (дочерний)
    private Camera mainCamera;
    private Collider shapeCollider; // Коллайдер самой формы для детекта мыши

    private void Start()
    {
        mainCamera = Camera.main;

        // Находим коллайдер на этом же объекте
        shapeCollider = GetComponent<Collider>();
        if (shapeCollider == null)
        {
            Debug.LogError("ВНИМАНИЕ! На объекте " + gameObject.name + " нет Collider! Добавь его, иначе линия не будет рисоваться.");
        }

        dottedLineRenderer = GetComponent<LineRenderer>();
        dottedLineRenderer.positionCount = 0;

        // ЖЁСТКО ВКЛЮЧАЕМ МИРОВЫЕ КООРДИНАТЫ ДЛЯ СТАБИЛЬНОСТИ
        dottedLineRenderer.useWorldSpace = true;

        // Автоматически создаем объект для красивой линии
        GameObject childObj = new GameObject("SolidLine_Animated");
        childObj.transform.SetParent(transform, false);

        solidLineRenderer = childObj.AddComponent<LineRenderer>();

        solidLineRenderer.useWorldSpace = true;
        solidLineRenderer.alignment = dottedLineRenderer.alignment;
        solidLineRenderer.widthCurve = dottedLineRenderer.widthCurve;
        solidLineRenderer.colorGradient = dottedLineRenderer.colorGradient;
        solidLineRenderer.sharedMaterial = solidLineMaterial;
        solidLineRenderer.textureMode = LineTextureMode.Stretch;

        solidLineRenderer.widthMultiplier = dottedLineRenderer.widthMultiplier * solidLineWidthMultiplier;
        solidLineRenderer.positionCount = 0;

        foreach (var point in points)
        {
            if (point != null) point.Init(this);
        }
    }

    private void Update()
    {
        if (!completed && lastVisitedIndex != -1 && Input.GetMouseButton(0))
        {
            UpdateTrailingLine();
        }
    }

    private void UpdateTrailingLine()
    {
        // Стреляем физическим лучом из камеры в позицию мыши
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Проверяем удар только об коллайдер нашей формы
        if (shapeCollider != null && shapeCollider.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 hitPoint = hit.point;

            // Немного сдвигаем точку к камере, чтобы она не утопала в объекте
            hitPoint += mainCamera.transform.forward * zOffset;

            dottedLineRenderer.positionCount = visitedCount + 1;
            dottedLineRenderer.SetPosition(visitedCount, hitPoint);
        }
    }

    private void OnMouseEnter()
    {
        if (!completed)
        {
            CutManager.Instance.StartCut(this);
            if (CursorVisualController.Instance != null)
                CursorVisualController.Instance.SetInteractableState(true);
        }
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

        Vector3 pointPos = point.transform.position;
        // Чуть сдвигаем точку к камере
        pointPos += mainCamera.transform.forward * zOffset;

        dottedLineRenderer.positionCount = visitedCount;
        dottedLineRenderer.SetPosition(visitedCount - 1, pointPos);
    }

    public void ResetProgress()
    {
        if (completed) return;

        visitedCount = 0;
        lastVisitedIndex = -1;
        firstVisitedIndex = -1;
        direction = 0;

        foreach (var point in points)
        {
            if (point != null) point.ResetPoint();
        }

        if (dottedLineRenderer != null) dottedLineRenderer.positionCount = 0;
        if (solidLineRenderer != null) solidLineRenderer.positionCount = 0;
    }

    private void CloseAndFinish(CutPoint startPoint)
    {
        completed = true;

        Vector3 startPos = startPoint.transform.position;
        startPos += mainCamera.transform.forward * zOffset;

        dottedLineRenderer.positionCount = visitedCount + 1;
        dottedLineRenderer.SetPosition(visitedCount, startPos);

        if (CursorVisualController.Instance != null)
            CursorVisualController.Instance.SetInteractableState(false);

        StartCoroutine(AnimateCutAndSpawn());
    }

    private IEnumerator AnimateCutAndSpawn()
    {
        int totalPositions = dottedLineRenderer.positionCount;
        Vector3[] pathPositions = new Vector3[totalPositions];
        dottedLineRenderer.GetPositions(pathPositions);

        float elapsedTime = 0f;

        while (elapsedTime < cutAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / cutAnimationDuration);

            float currentPathTarget = progress * (totalPositions - 1);
            int pointsToDraw = Mathf.FloorToInt(currentPathTarget) + 1;

            solidLineRenderer.positionCount = pointsToDraw + 1;

            for (int i = 0; i <= pointsToDraw; i++)
            {
                if (i < totalPositions)
                {
                    solidLineRenderer.SetPosition(i, pathPositions[i]);
                }
            }

            if (pointsToDraw < totalPositions)
            {
                int lastPointIndex = pointsToDraw;
                Vector3 startSeg = pathPositions[lastPointIndex - 1];
                Vector3 endSeg = pathPositions[lastPointIndex];
                float segmentProgress = currentPathTarget - (lastPointIndex - 1);

                Vector3 currentScissorsPos = Vector3.Lerp(startSeg, endSeg, segmentProgress);
                solidLineRenderer.SetPosition(lastPointIndex, currentScissorsPos);
            }

            yield return null;
        }

        solidLineRenderer.positionCount = totalPositions;
        solidLineRenderer.SetPositions(pathPositions);

        yield return new WaitForSeconds(0.1f);

        foreach (var point in points)
        {
            if (point != null) point.gameObject.SetActive(false);
        }

        dottedLineRenderer.positionCount = 0;
        solidLineRenderer.positionCount = 0;
        dottedLineRenderer.enabled = false;
        solidLineRenderer.enabled = false;

        Instantiate(resultPrefab, spawnPoint != null ? spawnPoint.position : transform.position + Vector3.right * 2f, Quaternion.identity);
        Destroy(gameObject);
    }
}
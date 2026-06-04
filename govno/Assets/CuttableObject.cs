using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class CuttableShape : MonoBehaviour
{
    public List<CutPoint> points;
    public GameObject resultPrefab;
    public Transform spawnPoint;

    [Header("Настройки партиклов")]
    [SerializeField] private GameObject spawnParticlesPrefab;

    [Header("Настройки анимации")]
    [SerializeField] private Material solidLineMaterial;
    [SerializeField] private float cutAnimationDuration = 0.8f;
    [Range(0.05f, 2f)]
    [SerializeField] private float solidLineWidthMultiplier = 0.3f;
    [SerializeField] private float zLocalOffset = -0.1f;

    private int visitedCount = 0;
    private bool completed = false;
    private int lastVisitedIndex = -1;
    private int firstVisitedIndex = -1;
    private int direction = 0;

    private LineRenderer dottedLineRenderer;
    private LineRenderer solidLineRenderer;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        dottedLineRenderer = GetComponent<LineRenderer>();
        dottedLineRenderer.positionCount = 0;
        dottedLineRenderer.useWorldSpace = false;
        dottedLineRenderer.alignment = LineAlignment.View;

        GameObject childObj = new GameObject("SolidLine_Animated");
        childObj.transform.SetParent(transform, false);

        solidLineRenderer = childObj.AddComponent<LineRenderer>();
        solidLineRenderer.useWorldSpace = false;
        solidLineRenderer.alignment = LineAlignment.View;

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
        Vector3 refWorldPos = points[firstVisitedIndex].transform.position;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(refWorldPos);
        Vector3 mouseScreenWithDepth = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mouseScreenWithDepth);
        Vector3 localMousePos = transform.InverseTransformPoint(worldMousePos);

        localMousePos.z = zLocalOffset;

        dottedLineRenderer.positionCount = visitedCount + 1;
        dottedLineRenderer.SetPosition(visitedCount, localMousePos);
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

        Vector3 localPointPos = transform.InverseTransformPoint(point.transform.position);
        localPointPos.z = zLocalOffset;

        dottedLineRenderer.positionCount = visitedCount;
        dottedLineRenderer.SetPosition(visitedCount - 1, localPointPos);
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

        Vector3 localStartPos = transform.InverseTransformPoint(startPoint.transform.position);
        localStartPos.z = zLocalOffset;

        dottedLineRenderer.positionCount = visitedCount + 1;
        dottedLineRenderer.SetPosition(visitedCount, localStartPos);

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
                if (i < totalPositions) solidLineRenderer.SetPosition(i, pathPositions[i]);
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

        Vector3 finalSpawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.right * 2f;
        GameObject prefabToSpawn = resultPrefab;

        if (CutValidator.Instance != null)
        {
            List<CutPoint> visitedPointsList = new List<CutPoint>();
            foreach (var point in points)
            {
                if (point.Visited) visitedPointsList.Add(point);
            }

            // Передаем валидатору то, что нарезали, и вообще ВСЕ точки этой формы
            prefabToSpawn = CutValidator.Instance.ValidateCut(visitedPointsList, points);
        }

        if (prefabToSpawn != null) Instantiate(prefabToSpawn, finalSpawnPos, Quaternion.identity);

        if (spawnParticlesPrefab != null)
        {
            Vector3 particlePos = finalSpawnPos + Camera.main.transform.forward * -0.1f;
            GameObject fx = Instantiate(spawnParticlesPrefab, particlePos, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Destroy(gameObject);
    }
}
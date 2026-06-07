using UnityEngine;
using System.Collections.Generic;

public class CutValidator : MonoBehaviour
{
    [Header("Настройки победы для КОНКРЕТНОЙ фигуры")]
    [Tooltip("Сколько УШЕК (Flaps) должен собрать игрок для правильной склейки именно этой фигуры?")]
    [SerializeField] private int requiredFlapCount = 4; // Поставил 4 для твоей пирамиды с 1 точкой на ухо

    [Header("Префабы результатов")]
    [Tooltip("Префаб успешной собранной 3D фигуры")]
    [SerializeField] private GameObject successPrefab;

    [Tooltip("Префаб брака (если ушек слишком много/мало или отрезана основа)")]
    [SerializeField] private GameObject failurePrefab;

    public GameObject ValidateCut(List<CutPoint> visitedPoints, List<CutPoint> allPuzzlePoints)
    {
        int visitedCoreCount = 0;
        int visitedFlapCount = 0;
        int totalCoreCount = 0;

        // 1. Считаем типы точек
        foreach (var point in allPuzzlePoints)
        {
            if (point != null && point.pointType == CutPoint.PointType.Core)
            {
                totalCoreCount++;
            }
        }

        foreach (var point in visitedPoints)
        {
            if (point == null) continue;
            if (point.pointType == CutPoint.PointType.Core) visitedCoreCount++;
            if (point.pointType == CutPoint.PointType.Flap) visitedFlapCount++;
        }

        // 2. Проверяем условия победы
        bool collectedAllCore = (visitedCoreCount == totalCoreCount);
        bool collectedExactFlaps = (visitedFlapCount == requiredFlapCount);

        if (collectedAllCore && collectedExactFlaps)
        {
            Debug.Log($"<color=green>[Валидатор {gameObject.name}]: Успех! Фигура собрана правильно.</color>");
            return successPrefab;
        }
        else
        {
            Debug.Log($"<color=red>[Валидатор {gameObject.name}]: Брак! Граней: {visitedCoreCount}/{totalCoreCount}, Ушек: {visitedFlapCount}/{requiredFlapCount}</color>");
            return failurePrefab;
        }
    }
}
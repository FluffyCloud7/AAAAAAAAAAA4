using UnityEngine;
using System.Collections.Generic;

public class CutValidator : MonoBehaviour
{
    public static CutValidator Instance { get; private set; }

    [Header("Настройки победы")]
    [Tooltip("Сколько УШЕК (Flaps) должен собрать игрок для правильной склейки?")]
    [SerializeField] private int requiredFlapCount = 3;

    [Header("Префабы результатов")]
    [Tooltip("Префаб успешной собранной 3D фигуры")]
    [SerializeField] private GameObject successPrefab;

    [Tooltip("Префаб брака (если ушек слишком много/мало или отрезана основа)")]
    [SerializeField] private GameObject failurePrefab;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject ValidateCut(List<CutPoint> visitedPoints, List<CutPoint> allPuzzlePoints)
    {
        int visitedCoreCount = 0;
        int visitedFlapCount = 0;

        int totalCoreCount = 0;

        // 1. Считаем, сколько всего Core-точек на уровне, и сколько каких точек посетил игрок
        foreach (var point in allPuzzlePoints)
        {
            if (point.pointType == CutPoint.PointType.Core)
            {
                totalCoreCount++;
            }
        }

        foreach (var point in visitedPoints)
        {
            if (point.pointType == CutPoint.PointType.Core) visitedCoreCount++;
            if (point.pointType == CutPoint.PointType.Flap) visitedFlapCount++;
        }

        // 2. Проверяем главное условие победы
        // Игрок должен посетить ВСЕ обязательные точки (основу)
        bool collectedAllCore = (visitedCoreCount == totalCoreCount);

        // Игрок должен собрать СТРОГО нужное количество ушек
        bool collectedExactFlaps = (visitedFlapCount == requiredFlapCount);

        if (collectedAllCore && collectedExactFlaps)
        {
            Debug.Log("<color=green>[Валидатор]: Успех! Собраны все грани и ровно нужное число ушек.</color>");
            return successPrefab;
        }
        else
        {
            Debug.Log($"<color=red>[Валидатор]: Брак! Граней: {visitedCoreCount}/{totalCoreCount}, Ушек: {visitedFlapCount}/{requiredFlapCount}</color>");
            return failurePrefab;
        }
    }
}
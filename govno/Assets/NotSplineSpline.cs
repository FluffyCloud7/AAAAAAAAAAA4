using UnityEngine;

public class HexPlatformAutoFence : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject postPrefab; // Сюда префаб столба
    public GameObject wallPrefab; // Сюда префаб решетки

    [ContextMenu("Сгенерировать забор автоматически")]
    public void AutoGenerate()
    {
        // 1. Ищем меш платформы
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("Скрипт должен висеть на объекте с Mesh Filter (на самой платформе)!");
            return;
        }

        // Очищаем старый забор, если он был
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.StartsWith("Auto_"))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        // 2. Вытаскиваем вершины меша
        Vector3[] vertices = meshFilter.sharedMesh.vertices;
        System.Collections.Generic.List<Vector3> uniqueTopVertices = new System.Collections.Generic.List<Vector3>();

        // Нам нужны только уникальные вершины, которые находятся НАВЕРХУ платформы
        float maxWeight = -999f;
        foreach (Vector3 v in vertices)
        {
            if (v.y > maxWeight) maxWeight = v.y; // Ищем самую высокую точку (уровень пола)
        }

        foreach (Vector3 v in vertices)
        {
            // Переводим локальные координаты вершины в мировые
            Vector3 worldPos = transform.TransformPoint(v);

            // Фильтруем по высоте (только верхние углы) и убираем дубликаты вершин
            if (Mathf.Abs(v.y - maxWeight) < 0.01f && !VectorListContains(uniqueTopVertices, worldPos, 0.1f))
            {
                uniqueTopVertices.Add(worldPos);
            }
        }

        // Сортируем вершины по кругу (чтобы забор шёл последовательно, а не зигзагами)
        Vector3 center = transform.position;
        uniqueTopVertices.Sort((a, b) => {
            float angleA = Mathf.Atan2(a.z - center.z, a.x - center.x);
            float angleB = Mathf.Atan2(b.z - center.z, b.x - center.x);
            return angleA.CompareTo(angleB);
        });

        if (uniqueTopVertices.Count < 3)
        {
            Debug.LogError($"Найдено слишком мало верхних углов ({uniqueTopVertices.Count}). Проверь геометрию платформы.");
            return;
        }

        // 3. Спавним забор по найденным углам
        for (int i = 0; i < uniqueTopVertices.Count; i++)
        {
            Vector3 currentCorner = uniqueTopVertices[i];
            Vector3 nextCorner = uniqueTopVertices[(i + 1) % uniqueTopVertices.Count];

            // Ставим столб
            if (postPrefab != null)
            {
                GameObject post = Instantiate(postPrefab, currentCorner, Quaternion.identity, transform);
                post.name = $"Auto_Post_{i}";
                post.transform.rotation = Quaternion.LookRotation(nextCorner - currentCorner);
            }

            // Натягиваем решетку
            if (wallPrefab != null)
            {
                Vector3 dir = nextCorner - currentCorner;
                GameObject wall = Instantiate(wallPrefab, currentCorner, Quaternion.LookRotation(dir), transform);
                wall.name = $"Auto_Wall_{i}";

                // Авто-подгон масштаба под длину грани платформы
                Vector3 scale = wall.transform.localScale;
                scale.z = dir.magnitude;
                wall.transform.localScale = scale;
            }
        }

        Debug.Log($"Забор успешно построен! Найдено углов: {uniqueTopVertices.Count}");
    }

    private bool VectorListContains(System.Collections.Generic.List<Vector3> list, Vector3 point, float threshold)
    {
        foreach (Vector3 p in list)
        {
            if (Vector3.Distance(p, point) < threshold) return true;
        }
        return false;
    }
}
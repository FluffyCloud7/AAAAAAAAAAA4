using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SpiralTextController : MonoBehaviour
{
    [Header("Настройки Спирали")]
    public string[] words = { "тоска", "одиночество", "утрата" };
    public TMP_FontAsset fontAsset;
    public float spawnInterval = 1.5f; // Как часто появляются слова
    public float moveSpeed = 2f;       // Скорость удаления от центра
    public float spiralTightness = 0.5f; // Насколько плотная спираль (шаг)
    public float rotationSpeed = 50f;   // Скорость закручивания

    [Header("Масштаб и Прозрачность")]
    public float startSize = 0.2f;
    public float endSize = 2.0f;
    public float maxRadius = 10f;       // Радиус, где слово полностью исчезает
    public float fadeStartRadius = 7f;   // Радиус, с которого начинается плавное исчезновение

    private float spawnTimer;
    private List<MovingWord> activeWords = new List<MovingWord>();

    private class MovingWord
    {
        public GameObject obj;
        public TextMeshPro tmp;
        public float currentRadius;
        public float currentAngle;
    }

    void Update()
    {
        // Спавн новых слов
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnWord();
            spawnTimer = 0f;
        }

        // Обновление существующих слов
        for (int i = activeWords.Count - 1; i >= 0; i--)
        {
            var word = activeWords[i];

            // Двигаем по спирали наружу
            word.currentRadius += moveSpeed * Time.deltaTime;
            word.currentAngle += rotationSpeed * Time.deltaTime / (word.currentRadius + 0.1f); // Чем дальше, тем медленнее угловая скорость

            // Проверка на вылет за границы
            if (word.currentRadius >= maxRadius)
            {
                Destroy(word.obj);
                activeWords.RemoveAt(i);
                continue;
            }

            // Позиция по спирали Архимеда
            float rad = word.currentAngle * Mathf.Deg2Rad;
            float x = (word.currentRadius * spiralTightness) * Mathf.Cos(rad);
            float y = (word.currentRadius * spiralTightness) * Mathf.Sin(rad);
            word.obj.transform.localPosition = new Vector3(x, y, 0);

            // Динамический масштаб
            float progress = word.currentRadius / maxRadius;
            float size = Mathf.Lerp(startSize, endSize, progress);
            word.obj.transform.localScale = new Vector3(size, size, 1);

            // Рассчитываем альфу (плавное исчезновение в конце и появление в начале)
            float alpha = 1f;
            if (word.currentRadius < 1f)
                alpha = word.currentRadius; // Плавное появление в самом центре
            else if (word.currentRadius > fadeStartRadius)
                alpha = 1f - ((word.currentRadius - fadeStartRadius) / (maxRadius - fadeStartRadius));

            // Деформация букв по дуге
            DeformTextAlongArc(word.tmp, word.currentRadius, alpha);
        }
    }

    void SpawnWord()
    {
        GameObject go = new GameObject("SpiralWord");
        go.transform.SetParent(this.transform);

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.font = fontAsset;
        tmp.text = words[Random.Range(0, words.Length)];
        tmp.alignment = TextAlignmentOptions.Center;

        // Отключаем лишнее, чтобы не ломало вершины
        tmp.extraPadding = true;

        MovingWord newWord = new MovingWord
        {
            obj = go,
            tmp = tmp,
            currentRadius = 0.1f,
            currentAngle = Random.Range(0f, 360f) // Случайный начальный угол, чтобы летели во все стороны
        };

        activeWords.Add(newWord);
    }

    void DeformTextAlongArc(TextMeshPro textComponent, float radius, float alpha)
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        // Изгиб зависит от радиуса: в центре (радиус мал) гнет сильно, на краю — почти не гнет
        float curvature = 15f / (radius + 0.5f);

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

            // Находим центр конкретной буквы
            float charCenterX = (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2f;

            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = vertices[vertexIndex + j];

                // Магия изгиба: смещаем Y в зависимости от X буквы, создавая дугу
                float xOffset = orig.x - charCenterX;
                float theta = charCenterX * curvature * Mathf.Deg2Rad;

                float sin = Mathf.Sin(theta);
                float cos = Mathf.Cos(theta);

                // Применяем поворот и изгиб для вершины
                float newX = cos * xOffset + (Mathf.Sin(theta) / (curvature + 0.001f));
                float newY = sin * xOffset + (cos / (curvature + 0.001f)) - (1f / (curvature + 0.001f));

                vertices[vertexIndex + j] = new Vector3(newX, orig.y + newY, orig.z);

                // Применяем плавное увядание цвета (Alpha)
                Color32 c = colors[vertexIndex + j];
                colors[vertexIndex + j] = new Color32(c.r, c.g, c.b, (byte)(alpha * 255));
            }
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
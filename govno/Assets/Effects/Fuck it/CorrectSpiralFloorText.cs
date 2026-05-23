using UnityEngine;
using System.Collections.Generic;

public class CorrectSpiralFloorText : MonoBehaviour
{
    [Header("Твои Прямые PNG Текстуры (ДО активации)")]
    public Texture2D[] wordTextures;
    public Color startColor = Color.white;
    public Color endColor = Color.red;

    [Header("Текстуры и Цвета (ПОСЛЕ активации)")]
    public Texture2D[] activatedTextures;
    public Color activatedStartColor = Color.green;
    public Color activatedEndColor = Color.cyan;

    [Header("Скорость перекрашивания старых слов")]
    [Tooltip("За сколько секунд старые слова полностью перекрасятся в новые цвета при активации")]
    public float colorSwitchSpeed = 0.5f;

    [Header("Параметры Поезда")]
    public Material spiralMaterial;
    public float moveSpeed = 1.0f;
    public float wordSpacing = 0.5f;
    public float spiralTightness = 0.16f;
    public float maxRadius = 4f;

    [Header("Динамический Масштаб")]
    [Range(0.01f, 2f)] public float baseScale = 0.4f;
    [Range(0.1f, 3f)] public float widthMultiplier = 1.0f;
    public float startSizeMultiplier = 0.2f;
    public float endSizeMultiplier = 1.0f;
    public float fadeStartRadius = 3.0f;

    private List<TrainWord> train = new List<TrainWord>();
    private int nextSpawnIndex = 0;
    private float distanceTravelled = 0f;
    private float nextSpawnDistance = 0f;

    private bool isActivated = false;
    private float activationProgress = 0f;

    private class TrainWord
    {
        public GameObject obj;
        public MeshRenderer renderer;
        public Material instancedMaterial;
        public float distanceOffset;
        public float aspectRatio;
        public bool spawnedAfterActivation;
    }

    // МАГИЯ СТАРТА: Раскручиваем спираль до начала игры
    void Start()
    {
        if (wordTextures == null || wordTextures.Length == 0 || spiralMaterial == null) return;

        // Приблизительное расстояние, необходимое для полного заполнения спирали до краев
        float approxTotalLength = 0.5f * maxRadius * (3.0f * 2f * Mathf.PI);

        // Виртуальное время, за которое поезд проехал бы этот путь
        float virtualTime = approxTotalLength / moveSpeed;

        // Шаг симуляции (чем меньше шаг, тем точнее распределятся слова на старте)
        float simStep = 0.05f;
        float accumulatedTime = 0f;

        // Крутим виртуальный цикл Update, пока спираль не заполнится
        while (accumulatedTime < virtualTime)
        {
            distanceTravelled += moveSpeed * simStep;

            if (distanceTravelled >= nextSpawnDistance)
            {
                SpawnWord(wordTextures);
                nextSpawnDistance = distanceTravelled + wordSpacing;
            }

            accumulatedTime += simStep;
        }

        // После того как сцена заполнилась стартовыми вагонами, делаем один быстрый проход,
        // чтобы сразу поставить их в правильные позиции на полу до первого рендера
        UpdateTrainPositions();
    }

    public void ActivateCheckpoint()
    {
        if (isActivated) return;
        isActivated = true;
        nextSpawnIndex = 0;
    }

    void Update()
    {
        // Двигаем таймер перекрашивания
        if (isActivated && activationProgress < 1f)
        {
            activationProgress += (1f / Mathf.Max(0.01f, colorSwitchSpeed)) * Time.deltaTime;
            activationProgress = Mathf.Clamp01(activationProgress);
        }

        Texture2D[] currentActivePool = isActivated ? activatedTextures : wordTextures;
        if (currentActivePool == null || currentActivePool.Length == 0 || spiralMaterial == null) return;

        // Обычное движение в реальном времени
        distanceTravelled += moveSpeed * Time.deltaTime;

        if (distanceTravelled >= nextSpawnDistance)
        {
            SpawnWord(currentActivePool);
            nextSpawnDistance = distanceTravelled + wordSpacing;
        }

        UpdateTrainPositions();
    }

    // Вынесли логику обновления позиций и цветов в отдельный метод, чтобы вызывать и в Start, и в Update
    void UpdateTrainPositions()
    {
        for (int i = train.Count - 1; i >= 0; i--)
        {
            var car = train[i];
            float carDistance = distanceTravelled - car.distanceOffset;

            Vector3 pos = GetSpiralPoint(carDistance);
            float currentRadius = pos.magnitude;

            if (currentRadius >= maxRadius || float.IsNaN(currentRadius))
            {
                Destroy(car.obj);
                train.RemoveAt(i);
                continue;
            }

            car.obj.transform.localPosition = new Vector3(pos.x, 0.03f, pos.z);

            Vector3 nextPos = GetSpiralPoint(carDistance + 0.05f);
            Vector3 dir = nextPos - pos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                car.obj.transform.localRotation = Quaternion.Euler(90f, angle + 90f, 0f);
            }

            float progress = Mathf.Clamp01(currentRadius / maxRadius);
            float finalHeight = baseScale * Mathf.Lerp(startSizeMultiplier, endSizeMultiplier, progress);
            float finalWidth = finalHeight * car.aspectRatio * widthMultiplier;

            if (float.IsNaN(finalWidth) || float.IsNaN(finalHeight))
            {
                finalWidth = 0.1f; finalHeight = 0.1f;
            }

            car.obj.transform.localScale = new Vector3(finalWidth, finalHeight, 1f);

            Color oldEraColor = Color.Lerp(startColor, endColor, progress);
            Color newEraColor = Color.Lerp(activatedStartColor, activatedEndColor, progress);

            Color currentColor = car.spawnedAfterActivation ? newEraColor : Color.Lerp(oldEraColor, newEraColor, activationProgress);

            // Прозрачность (Fade)
            if (currentRadius > fadeStartRadius)
            {
                float fadeProgress = (currentRadius - fadeStartRadius) / (maxRadius - fadeStartRadius);
                currentColor.a = Mathf.Lerp(currentColor.a, 0f, Mathf.Clamp01(fadeProgress));
            }
            else if (carDistance < 0.3f)
            {
                currentColor.a = Mathf.Min(currentColor.a, carDistance / 0.3f);
            }

            car.instancedMaterial.SetColor("_BaseColor", currentColor);
        }
    }

    Vector3 GetSpiralPoint(float dist)
    {
        if (dist < 0) dist = 0;
        float theta = Mathf.Sqrt(2f * dist / spiralTightness);
        float r = spiralTightness * theta;
        return new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
    }

    void SpawnWord(Texture2D[] currentPool)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Train_Word_Quad";
        go.transform.SetParent(this.transform);
        Destroy(go.GetComponent<Collider>());

        MeshRenderer renderer = go.GetComponent<MeshRenderer>();

        Material uniqueMat = new Material(spiralMaterial);
        Texture2D currentTex = currentPool[nextSpawnIndex];
        uniqueMat.SetTexture("_BaseMap", currentTex);
        renderer.material = uniqueMat;

        float aspect = 1f;
        if (currentTex != null && currentTex.height > 0)
        {
            aspect = (float)currentTex.width / currentTex.height;
        }

        train.Add(new TrainWord
        {
            obj = go,
            renderer = renderer,
            instancedMaterial = uniqueMat,
            distanceOffset = distanceTravelled,
            aspectRatio = aspect,
            spawnedAfterActivation = isActivated
        });

        nextSpawnIndex = (nextSpawnIndex + 1) % currentPool.Length;
    }
}
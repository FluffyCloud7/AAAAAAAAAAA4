using UnityEngine;

public class PaperGrassWind : MonoBehaviour
{
    [Header("Настройки колыхания")]
    public float windSpeed = 2f;       // Скорость ветра
    public float windStrength = 5f;    // Силы наклона (в градусах)

    private Quaternion baseRotation;
    private float offsetX;
    private float offsetZ;

    private void Start()
    {
        // Запоминаем исходный поворот травы, который ей дала кисть при спавне
        baseRotation = transform.rotation;

        // Создаем уникальное смещение по координатам, чтобы соседние травинки качались невпопад
        offsetX = transform.position.x * 0.5f;
        offsetZ = transform.position.z * 0.5f;
    }

    private void Update()
    {
        // Получаем плавный хаотичный шум времени для осей X и Z
        float time = Time.time * windSpeed;

        // Шум Перлина дает естественные порывы ветра
        float windX = Mathf.PerlinNoise(time + offsetX, time + offsetZ) * 2f - 1f;
        float windZ = Mathf.PerlinNoise(time - offsetX, time - offsetZ) * 2f - 1f;

        // Создаем небольшой наклон в градусах
        Quaternion tilt = Quaternion.Euler(windX * windStrength, 0f, windZ * windStrength);

        // Применяем наклон поверх базового поворота травы
        transform.rotation = baseRotation * tilt;
    }
}
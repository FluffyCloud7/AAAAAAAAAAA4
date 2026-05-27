using UnityEngine;

public class WindSimulation : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Настройки ветра")]
    public float windStrength = 0.5f; // Сила ветра (начни с маленьких значений)
    public float windSpeed = 1.5f;    // Как часто меняются порывы ветра

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Используем шум Перлина (Mathf.PerlinNoise) для плавных, но хаотичных порывов
        // В отличие от синусоиды, шум выглядит более естественно и нециклично
        float windX = Mathf.PerlinNoise(Time.time * windSpeed, 0f) * 2f - 1f;
        float windZ = Mathf.PerlinNoise(0f, Time.time * windSpeed) * 2f - 1f;

        // Собираем вектор силы (ветер дует горизонтально, Y не трогаем)
        Vector3 windForce = new Vector3(windX, 0f, windZ) * windStrength;

        // Применяем силу каждый физический кадр
        rb.AddForce(windForce, ForceMode.Force);
    }
}
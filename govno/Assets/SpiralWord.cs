using UnityEngine;

public class SpiralWord : MonoBehaviour
{
    private float angle;
    private float currentRadius;
    private float minRadius = 0.1f;

    private float rotationSpeed;
    private float expansionSpeed;
    private float lifetime;      // Полное время жизни в секундах
    private float lifeTimer = 0f; // Сколько секунд уже прожило слово

    public SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isInitialized = false;

    public void Initialize(Sprite sprite, Color color, float targetLifetime, float rotSpeed, float expSpeed)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;

        lifetime = targetLifetime;
        rotationSpeed = rotSpeed;
        expansionSpeed = expSpeed;

        currentRadius = minRadius;
        angle = Random.Range(0f, Mathf.PI * 2f);
        lifeTimer = 0f;

        originalScale = transform.localScale;

        // Старт из невидимости и нулевого размера
        transform.localScale = Vector3.zero;
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Линейно наращиваем таймер жизни
        lifeTimer += Time.deltaTime;

        // Считаем процент прожитой жизни от 0 (родился) до 1 (пора умирать)
        float lifePercent = Mathf.Clamp01(lifeTimer / lifetime);

        // 1. Движение по спирали
        angle += rotationSpeed * Time.deltaTime;
        currentRadius += expansionSpeed * Time.deltaTime;

        // 2. Плавный рост размера и проявление (первые 20% времени жизни)
        if (lifePercent < 0.2f)
        {
            float fadeInProgress = lifePercent / 0.2f;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, fadeInProgress);

            Color c = spriteRenderer.color;
            c.a = fadeInProgress;
            spriteRenderer.color = c;
        }
        // 3. Плавное затухание (последние 20% времени жизни)
        else if (lifePercent > 0.8f)
        {
            float fadeOutProgress = (1f - lifePercent) / 0.2f;

            Color c = spriteRenderer.color;
            c.a = fadeOutProgress;
            spriteRenderer.color = c;

            if (c.a <= 0.001f)
            {
                Destroy(gameObject);
                return;
            }
        }
        // В середине жизни держим форму и цвет
        else
        {
            transform.localScale = originalScale;
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        // Если время вышло совсем — уничтожаем объект
        if (lifePercent >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 4. Позиция на полу XZ
        Transform parentTransform = transform.parent;
        Vector3 center = parentTransform != null ? parentTransform.position : Vector3.zero;

        float x = center.x + Mathf.Cos(angle) * currentRadius;
        float z = center.z + Mathf.Sin(angle) * currentRadius;
        transform.position = new Vector3(x, center.y, z);

        // 5. Поворот основанием текста к центру
        Vector3 directionToCenter = center - transform.position;
        if (directionToCenter != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(directionToCenter.x, directionToCenter.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(90f, targetAngle, 0f);
        }
    }
}
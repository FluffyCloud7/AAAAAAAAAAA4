using UnityEngine;
using System.Collections;

public class FeatherFloating : MonoBehaviour
{
    [Header("Настройки парения")]
    public float bobSpeed = 2f;      // Скорость покачивания вверх-вниз
    public float bobHeight = 0.15f;  // Амплитуда покачивания
    public float rotateSpeed = 40f;  // Скорость вращения

    [Header("Настройки сжатия")]
    [Tooltip("За сколько секунд перо полностью сожмется в точку")]
    public float shrinkDuration = 1.0f;
    [Tooltip("Кривая сжатия. Сделай её резкой в конце для эффекта 'хлопка'")]
    public AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 startPos;
    private Vector3 initialScale;
    private bool isDissolving = false;

    void Start()
    {
        // Запоминаем локальные координаты для парения
        startPos = transform.localPosition;
        // Запоминаем оригинальный масштаб
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (isDissolving) return;

        // Математика парения (синусоида)
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // Вращение
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // Метод для вызова из чекпоинта
    public void CollectFeather()
    {
        if (isDissolving) return;
        StartCoroutine(ShrinkRoutine());
    }

    private IEnumerator ShrinkRoutine()
    {
        isDissolving = true;
        float elapsed = 0f;

        // В начале сжатия можно немного ускорить вращение для эффекта вихря
        float currentRotateSpeed = rotateSpeed * 3f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            // Нормированный прогресс от 0 до 1
            float t = elapsed / shrinkDuration;

            // Используем AnimationCurve для сочного сжатия
            // (Кривая EaseInOut даст плавный старт и резкий финиш)
            float curveT = shrinkCurve.Evaluate(t);

            // Плавно уменьшаем масштаб от оригинала к нулю
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, curveT);

            // Продолжаем вращать во время сжатия, но всё быстрее
            transform.Rotate(Vector3.up, currentRotateSpeed * Time.deltaTime);

            yield return null;
        }

        // В финале принудительно ставим ноль и выключаем
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
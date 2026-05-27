using UnityEngine;

public class FloatingColumn : MonoBehaviour
{
    [Header("Настройки парения")]
    [SerializeField] private float amplitude = 0.5f; // Высота взлета и падения (в метрах)
    [SerializeField] private float speed = 1.0f;     // Скорость движения

    private Vector3 startPosition;

    void Start()
    {
        // Запоминаем стартовую позицию в мировых координатах
        startPosition = transform.position;
    }

    void Update()
    {
        // Вычисляем смещение по оси Y с помощью синусоиды
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;

        // Обновляем позицию, сохраняя начальные X и Z мировых координат
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
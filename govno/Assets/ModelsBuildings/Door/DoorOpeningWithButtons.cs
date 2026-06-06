using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Настройки вращения")]
    [Tooltip("Угол, на который отклоняется открытая дверь (например, 90 или -90)")]
    [SerializeField] private float openAngle = 90f;
    [Tooltip("Скорость открытия двери")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Текущее состояние")]
    [SerializeField] private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // Запоминаем начальное вращение как "закрытое"
        closedRotation = transform.localRotation;

        // Считаем "открытое" вращение относительно начального по оси Y
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        // Сразу выставляем стартовую позицию без плавной анимации
        targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = targetRotation;
    }

    void Update()
    {
        // Каждым кадром плавно приближаем текущий поворот к целевому
        if (transform.localRotation != targetRotation)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    // Этот метод вызывается кнопкой через Unity Event
    public void Toggle()
    {
        isOpen = !isOpen;

        // Меняем цель для вращения
        targetRotation = isOpen ? openRotation : closedRotation;

        Debug.Log($"Дверь {gameObject.name} меняет позицию. Открыта: {isOpen}");
    }
}
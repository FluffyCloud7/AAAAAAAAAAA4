using UnityEngine;
using UnityEngine.Events;

public class TriggerButton : MonoBehaviour
{
    [Header("Настройки дверей")]
    [Tooltip("Перетащи сюда двери из иерархии, на которые должна влиять эта кнопка")]
    [SerializeField] private UnityEvent onButtonToggle;

    [Header("Визуальное состояние (необязательно)")]
    [SerializeField] private Vector3 pressedOffset = new Vector3(0, -0.1f, 0); // На сколько опускается кнопка
    [SerializeField] private float changeSpeed = 10f;

    private bool isPressed = false;      // Нажата ли кнопка в данный момент (фиксация)
    private bool playerInside = false;    // Стоит ли игрок прямо сейчас на кнопке

    private Vector3 initialPosition;
    private Vector3 targetPosition;

    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    void Update()
    {
        // Плавно опускаем/поднимаем кнопку для визуала
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * changeSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что на кнопку наступил именно игрок
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;

            // Меняем состояние кнопки на противоположное
            isPressed = !isPressed;

            // Дёргаем все привязанные двери
            onButtonToggle?.Invoke();

            // Визуальный сдвиг кнопки
            targetPosition = isPressed ? (initialPosition + pressedOffset) : initialPosition;

            Debug.Log($"Кнопка {gameObject.name} переключена. Текущее состояние: " + (isPressed ? "НАЖАТА" : "ОТЖАТА"));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Игрок ушёл, теперь при следующем наступлении кнопка снова сработает
            playerInside = false;
        }
    }
}
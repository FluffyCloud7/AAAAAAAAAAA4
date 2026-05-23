using UnityEngine;
using TMPro;
using System.Collections;

public class HintController : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private TextMeshPro textComponent; // Ссылка на твой ТМР текст на стене

    [Header("Настройки активации")]
    [Tooltip("Если true — текст появится сам через X секунд после старта. Если false — ждет триггера.")]
    public bool activateByTime = false;
    [Tooltip("Задержка перед появлением (актуально, если активируется по времени)")]
    public float delayBeforeShow = 2f;

    [Header("Настройки анимации текста")]
    [Tooltip("Скорость появления букв (меньше — быстрее пишется)")]
    public float writingSpeed = 0.04f;

    private bool isShowing = false;

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshPro>();

        if (textComponent == null)
        {
            Debug.LogError($"[HintController] На объекте {gameObject.name} не найден компонент TextMeshPro!");
            return;
        }

        // Изначально полностью прячем текст, обнуляя видимые символы
        textComponent.maxVisibleCharacters = 0;

        // ИСПРАВЛЕНИЕ: Вместо выключения всего объекта (SetActive(false)), 
        // мы просто включаем сам рендерер текста, чтобы корутина могла стартовать!
        textComponent.enabled = false;

        if (activateByTime)
        {
            StartCoroutine(WaitAndShowRoutine());
        }
    }

    // Этот метод будет вызывать наш триггер из зоны на полу
    public void TriggerActivation()
    {
        if (isShowing) return; // Защита, чтобы анимация не перезапускалась дважды

        // Останавливаем старые корутины на всякий случай и запускаем написание текста
        StopAllCoroutines();
        StartCoroutine(WriteTextRoutine());
    }

    private IEnumerator WaitAndShowRoutine()
    {
        yield return new WaitForSeconds(delayBeforeShow);
        StartCoroutine(WriteTextRoutine());
    }

    private IEnumerator WriteTextRoutine()
    {
        isShowing = true;

        // ИСПРАВЛЕНИЕ: Включаем компонент текста обратно
        textComponent.enabled = true;
        textComponent.maxVisibleCharacters = 0;

        textComponent.ForceMeshUpdate();

        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            textComponent.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(writingSpeed);
        }
    }

    // Публичный метод на случай, если тебе нужно будет кодом поменять текст подсказки налету
    public void ChangeHintText(string newText)
    {
        if (textComponent != null)
        {
            textComponent.text = newText;
            textComponent.maxVisibleCharacters = 0;
            isShowing = false;
        }
    }
}
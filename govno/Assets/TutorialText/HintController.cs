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
    private string originalText; // Запоминаем исходный текст

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshPro>();

        if (textComponent == null)
        {
            Debug.LogError($"[HintController] На объекте {gameObject.name} не найден компонент TextMeshPro!");
            return;
        }

        // Сохраняем текст, который ты написала в инспекторе
        originalText = textComponent.text;

        // Важно: Текст оставляем включенным (enabled = true), но прячем буквы.
        // Если выключить компонент, TMPro не сможет корректно просчитать объемы строк.
        textComponent.maxVisibleCharacters = 0;

        if (activateByTime)
        {
            StartCoroutine(WaitAndShowRoutine());
        }
    }

    public void TriggerActivation()
    {
        if (isShowing) return;

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

        // Шаг 1: Возвращаем полный текст и временно делаем все буквы видимыми
        textComponent.text = originalText;
        textComponent.maxVisibleCharacters = 9999;

        // Шаг 2: Жестко приказываем Unity просчитать финальные границы и переносы строк
        textComponent.ForceMeshUpdate();

        // Шаг 3: Теперь, когда строки зафиксированы на стене, узнаем точное число символов...
        int totalVisibleCharacters = textComponent.textInfo.characterCount;

        // ...и только сейчас мгновенно прячем их перед началом анимации
        textComponent.maxVisibleCharacters = 0;

        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            textComponent.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(writingSpeed);
        }
    }

    public void ChangeHintText(string newText)
    {
        originalText = newText;
        if (textComponent != null)
        {
            textComponent.text = newText;
            textComponent.maxVisibleCharacters = 0;
            isShowing = false;
        }
    }
}
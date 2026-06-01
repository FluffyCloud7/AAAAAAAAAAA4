using System.Collections;
using UnityEngine;

public class CharacterJuice : MonoBehaviour
{
    [Header("Visual Link")]
    [SerializeField] private Transform visualTransform; // Сюда кидаем дочерний визуал

    [Header("Squash & Stretch Settings")]
    [SerializeField] private float jumpStretchX = 0.7f; // Сужение при взлете
    [SerializeField] private float jumpStretchY = 1.4f; // Вытягивание вверх
    [SerializeField] private float landSquashX = 1.3f;  // Расплющивание при приземлении
    [SerializeField] private float landSquashY = 0.6f;  // Сжатие вниз
    [SerializeField] private float returnSpeed = 10f;   // Скорость возврата к 1,1,1

    private Vector3 originalScale = Vector3.one;

    void Start()
    {
        if (visualTransform == null) visualTransform = transform;
    }

    void Update()
    {
        // Плавно возвращаем модель к исходному размеру каждый кадр
        visualTransform.localScale = Vector3.Lerp(visualTransform.localScale, originalScale, Time.deltaTime * returnSpeed);
    }

    // Вызывать в момент нажатия кнопки прыжка (Jump)
    public void ApplyJumpStretch()
    {
        visualTransform.localScale = new Vector3(originalScale.x * jumpStretchX, originalScale.y * jumpStretchY, originalScale.z);
    }

    // Вызывать в момент касания земли (Grounded)
    public void ApplyLandSquash()
    {
        visualTransform.localScale = new Vector3(originalScale.x * landSquashX, originalScale.y * landSquashY, originalScale.z);
    }
}
using UnityEngine;
using System.Collections;

public class CharacterDeathEffect : MonoBehaviour
{
    [Header("Компоненты")]
    public Renderer characterRenderer;
    public ParticleSystem paperParticles;

    [Header("Материалы")]
    public Material dissolveMaterial; // Материал с Шейдер Графом
    public Material originalMaterial; // Сюда ПРЯМО ИЗ ИНСПЕКТОРА перетащи свой обычный материал с текстурой!

    [Header("Настройки анимации")]
    public float duration = 1.5f; // Время исчезновения

    private int dissolvePropID;
    private bool isEffectPlaying = false;

    // Переменные для хранения базового цвета оригинального материала
    private Color cachedOriginalColor;
    private bool colorCached = false;

    void Awake()
    {
        dissolvePropID = Shader.PropertyToID("_DissolveAmount");
    }

    private void CacheOriginalColor()
    {
        if (colorCached || originalMaterial == null) return;

        if (originalMaterial.HasProperty("_BaseColor"))
            cachedOriginalColor = originalMaterial.GetColor("_BaseColor");
        else if (originalMaterial.HasProperty("_Color"))
            cachedOriginalColor = originalMaterial.GetColor("_Color");
        else
            cachedOriginalColor = Color.white;

        colorCached = true;
    }

    public void PlayDeathEffect()
    {
        if (isEffectPlaying) return;

        StopAllCoroutines();
        StartCoroutine(DissolveRoutine());
    }

    public void ResetEffect()
    {
        StopAllCoroutines();
        isEffectPlaying = false;

        // Гарантированно возвращаем родной текстурный материал
        if (characterRenderer != null && originalMaterial != null)
        {
            // Используем .material, чтобы не ломать инстансы при изменении цвета
            characterRenderer.material = originalMaterial;
            ResetToOriginalColor();
        }
    }

    IEnumerator DissolveRoutine()
    {
        isEffectPlaying = true;

        if (characterRenderer != null && dissolveMaterial != null)
        {
            characterRenderer.material = dissolveMaterial;
            Material liveMaterial = characterRenderer.material;

            if (liveMaterial != null)
                liveMaterial.SetFloat(dissolvePropID, 0f);

            if (paperParticles != null)
            {
                paperParticles.Play();
            }

            float elapsedTime = 0f;
            float effectDuration = 1.4f;

            while (elapsedTime < effectDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / effectDuration;

                if (liveMaterial != null)
                {
                    liveMaterial.SetFloat(dissolvePropID, progress);
                }

                yield return null;
            }

            if (liveMaterial != null)
            {
                liveMaterial.SetFloat(dissolvePropID, 1f);
            }
        }

        isEffectPlaying = false;
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ МИГАНИЯ ---

    public void SetFlashColor(Color flashColor)
    {
        if (characterRenderer == null) return;

        CacheOriginalColor();
        Material currentMat = characterRenderer.material;

        if (currentMat.HasProperty("_BaseColor"))
            currentMat.SetColor("_BaseColor", flashColor);
        else if (currentMat.HasProperty("_Color"))
            currentMat.SetColor("_Color", flashColor);
    }

    public void ResetToOriginalColor()
    {
        if (characterRenderer == null) return;

        CacheOriginalColor();
        Material currentMat = characterRenderer.material;

        if (currentMat.HasProperty("_BaseColor"))
            currentMat.SetColor("_BaseColor", cachedOriginalColor);
        else if (currentMat.HasProperty("_Color"))
            currentMat.SetColor("_Color", cachedOriginalColor);
    }
}
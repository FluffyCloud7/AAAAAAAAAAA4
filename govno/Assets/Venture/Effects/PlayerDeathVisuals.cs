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

    void Awake()
    {
        dissolvePropID = Shader.PropertyToID("_DissolveAmount");
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
            characterRenderer.material = originalMaterial;
        }

        //if (paperParticles != null)
        //{
            //paperParticles.Stop();
            //paperParticles.Clear();
        //}
    }

    IEnumerator DissolveRoutine()
    {
        isEffectPlaying = true;

        // В момент смерти временно подменяем материал на растворяющийся Opaque
        if (characterRenderer != null && dissolveMaterial != null)
        {
            characterRenderer.material = dissolveMaterial;
            // Получаем доступ к ТОЛЬКО ЧТО ПОДМЕНЕННОМУ МАТЕРИАЛУ через Renderer.material
            Material liveMaterial = characterRenderer.material;

            // Сбрасываем растворение в 0 (персонаж полностью виден)
            if (liveMaterial != null)
                liveMaterial.SetFloat(dissolvePropID, 0f);

            if (paperParticles != null)
            {
                paperParticles.Play();
            }

            float elapsedTime = 0f;

            // duration должна быть равна или чуть МЕНЬШЕ, чем respawnDelay в PlayerHealth.
            // Я поставлю 1.4f, чтобы был запас в 0.1 сек перед телепортацией.
            float effectDuration = 1.4f;

            while (elapsedTime < effectDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / effectDuration; // Строго от 0 до 1

                // Передаем чистый прогресс от 0 до 1 в ПОДМЕНЕННЫЙ материал
                if (liveMaterial != null)
                {
                    liveMaterial.SetFloat(dissolvePropID, progress);
                }

                yield return null;
            }

            // ГАРАНТИЯ ИСЧЕЗНОВЕНИЯ: В самом конце зануляем ползунок в максимум (перс точно исчез)
            if (liveMaterial != null)
            {
                liveMaterial.SetFloat(dissolvePropID, 1f);
            }
        }

        isEffectPlaying = false;
    }
}
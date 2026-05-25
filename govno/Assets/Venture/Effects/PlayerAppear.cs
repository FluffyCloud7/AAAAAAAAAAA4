using UnityEngine;
using System.Collections;

public class CharacterAppearEffect : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Material appearMaterial;
    private Material originalMaterial;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1.4f;

    private void Awake()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (playerRenderer != null)
            originalMaterial = playerRenderer.sharedMaterial;
    }

    public void PlayAppearEffect()
    {
        if (playerRenderer != null && appearMaterial != null)
        {
            StopAllCoroutines();
            StartCoroutine(AppearRoutine());
        }
    }

    private IEnumerator AppearRoutine()
    {
        // —тавим временный материал по€влени€
        playerRenderer.material = appearMaterial;
        appearMaterial.SetFloat("_AppearAmount", 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            // ѕлавно про€вл€ем персонажа от 0 до 1
            appearMaterial.SetFloat("_AppearAmount", progress);
            yield return null;
        }

        // Ёффект закончен Ч возвращаем исходный чистый материал персонажа
        playerRenderer.material = originalMaterial;
    }
}
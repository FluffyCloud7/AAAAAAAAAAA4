using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public event System.Action<int, int> OnHealthChanged;

    // Мигание при неуязвимости
    private Renderer playerRenderer;
    private Coroutine blinkCoroutine;

    public event System.Action OnRespawn;

    public float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;


    public bool IsDead { get; private set; } = false;
    public event System.Action OnDeath;

    private Collider playerCollider;
    private CharacterController characterController;
    private Animator animator; // Ссылка на Аниматор

    // Ссылки на компоненты эффектов растворения и появления
    private CharacterDeathEffect deathEffect;
    private CharacterAppearEffect appearEffect; // НОВОЕ

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem respawnParticles; // Ссылка на партиклы для возрождения

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); // Ищем аниматор

        playerRenderer = GetComponentInChildren<Renderer>();

        // Инициализируем ссылки на эффекты
        deathEffect = GetComponent<CharacterDeathEffect>();
        appearEffect = GetComponent<CharacterAppearEffect>(); // НОВОЕ
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable || IsDead) return;

        currentHealth -= amount;
        SoundEffectManager.Play("HurtSound");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            // Обычная смерть (Тип 1 — колени, задержка перед спавном 1.5 сек)
            Die(1, 1.5f);
            return;
        }

        StartCoroutine(Invulnerability());
    }

    // Новый метод для внешнего триггера Бездны
    public void TakeVoidDamage()
    {
        if (IsDead) return;

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Смерть в бездне (Тип 2 — без коленей, респавн мгновенный)
        Die(2, 0f);
    }

    private void Die(int deathType, float respawnDelay)
    {
        if (IsDead) return;

        Debug.Log("Player died, type: " + deathType);
        IsDead = true;

        // Передаем тип смерти в Аниматор
        if (animator != null)
        {
            animator.SetInteger("DeathType", deathType);
        }

        // Запуск бумажного растворения при смерти
        if (deathEffect != null)
        {
            deathEffect.PlayDeathEffect();
        }

        OnDeath?.Invoke();

        StartCoroutine(RespawnAfterDelay(respawnDelay));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    public void Respawn()
    {
        Debug.Log("RespawnPlayer called");

        // Сброс эффекта растворения смерти
        if (deathEffect != null)
        {
            deathEffect.ResetEffect();
        }

        // ЗАПУСК ЭФФЕКТА БУМАЖНОГО ПОЯВЛЕНИЯ ПРИ РЕСПАВНЕ
        if (appearEffect != null)
        {
            appearEffect.PlayAppearEffect();
        }

        if (respawnController.Instance != null &&
            respawnController.Instance.respawnPoint != null)
        {
            Vector3 spawnPos = respawnController.Instance.respawnPoint.position;

            if (characterController != null)
            {
                characterController.enabled = false;
                transform.position = spawnPos;
                characterController.enabled = true;
            }
            else
            {
                transform.position = spawnPos;
            }
            OnRespawn?.Invoke();
        }

        // Включаем систему частиц в момент появления на точке спавна
        if (respawnParticles != null)
        {
            respawnParticles.Play();
        }

        // Логика возрождения для Аниматора
        if (animator != null)
        {
            animator.SetInteger("DeathType", 0);
            animator.SetTrigger("RespawnTrigger");
        }

        currentHealth = maxHealth;
        IsDead = false;

        if (playerCollider != null)
            playerCollider.enabled = true;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        StartCoroutine(Invulnerability());
    }

    // IEnumerator виден для Unity
    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine());
        yield return null;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;

            if (playerRenderer != null)
                playerRenderer.enabled = true;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (isInvulnerable)
        {
            if (playerRenderer != null)
                playerRenderer.enabled = !playerRenderer.enabled;

            yield return new WaitForSeconds(0.15f);
        }

        if (playerRenderer != null)
            playerRenderer.enabled = true;
    }

    public void RestoreFullHealth()
    {
        if (IsDead) return;

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
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
    private Animator animator; // Добавлено: Ссылка на Аниматор

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>(); // Добавлено: Ищем аниматор

        playerRenderer = GetComponentInChildren<Renderer>();
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

    // ВОЗВРАЩЕНО КАК В ОРИГИНАЛЕ: это IEnumerator, теперь Unity его видит
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
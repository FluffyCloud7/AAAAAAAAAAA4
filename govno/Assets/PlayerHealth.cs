using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public event System.Action<int, int> OnHealthChanged;

    // Настройки мигания при неуязвимости
    private Coroutine blinkCoroutine;

    [Header("Invisibility Blink Settings")]
    [SerializeField] private Color flashColor = Color.red; // Цвет мигания
    [SerializeField] private float blinkInterval = 0.15f; // Скорость мигания

    public event System.Action OnRespawn;

    public float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;

    public bool IsDead { get; private set; } = false;
    public event System.Action OnDeath;

    private Collider playerCollider;
    private CharacterController characterController;
    private Animator animator;

    // Ссылки на эффекты
    private CharacterDeathEffect deathEffect;
    private CharacterAppearEffect appearEffect;

    [Header("Effects Settings")]
    [SerializeField] private ParticleSystem respawnParticles;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        deathEffect = GetComponent<CharacterDeathEffect>();
        appearEffect = GetComponent<CharacterAppearEffect>();
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
            Die(1, 1.5f);
            return;
        }

        StartCoroutine(Invulnerability());
    }

    public void TakeVoidDamage()
    {
        if (IsDead) return;

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Die(2, 0f);
    }

    private void Die(int deathType, float respawnDelay)
    {
        if (IsDead) return;

        Debug.Log("Player died, type: " + deathType);
        IsDead = true;

        if (animator != null)
        {
            animator.SetInteger("DeathType", deathType);
        }

        // Останавливаем мигание перед смертью, чтобы материал не остался красным во время растворения
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (deathEffect != null)
        {
            deathEffect.ResetToOriginalColor();
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

        if (deathEffect != null)
        {
            deathEffect.ResetEffect(); // Сбросит на оригинальный материал и вернет исходный цвет
        }

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

        if (respawnParticles != null)
        {
            respawnParticles.Play();
        }

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

    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine());

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (deathEffect != null)
        {
            deathEffect.ResetToOriginalColor();
        }
    }

    private IEnumerator BlinkRoutine()
    {
        bool isRed = false;

        while (isInvulnerable)
        {
            if (deathEffect != null)
            {
                if (isRed)
                    deathEffect.ResetToOriginalColor();
                else
                    deathEffect.SetFlashColor(flashColor);

                isRed = !isRed;
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        if (deathEffect != null)
        {
            deathEffect.ResetToOriginalColor();
        }
    }

    public void RestoreFullHealth()
    {
        if (IsDead) return;

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
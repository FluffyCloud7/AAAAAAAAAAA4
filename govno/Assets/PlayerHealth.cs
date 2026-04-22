using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public event System.Action<int, int> OnHealthChanged;

    //Мигание при неуязвимости
    private Renderer playerRenderer;
    private Coroutine blinkCoroutine;

    public event System.Action OnRespawn;

    public float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;


    public bool IsDead { get; private set; } = false;
    public event System.Action OnDeath;

    private Collider playerCollider;
    private CharacterController characterController;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        playerCollider = GetComponent<Collider>();
        characterController = GetComponent<CharacterController>();

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
            Die();
            return;
        }

        StartCoroutine(Invulnerability());
    }

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

            // включаем рендер обратно
            if (playerRenderer != null)
                playerRenderer.enabled = true;
        }
    }

    private void Die()
    {
        if (IsDead) return;

        Debug.Log("Player died");
        IsDead = true;

        // Отключаем коллайдер, чтобы зона урона не мешала аааааа
        //if (playerCollider != null)
            //playerCollider.enabled = false;

        OnDeath?.Invoke();

        StartCoroutine(RespawnAfterDelay());
        //Respawn();
    }

    private IEnumerator RespawnAfterDelay()
    {
        // Если хочешь, можешь сделать задержку 0.5–1 сек.
        yield return new WaitForSeconds(0f);

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

        currentHealth = maxHealth;
        IsDead = false;

        if (playerCollider != null)
            playerCollider.enabled = true;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        StartCoroutine(Invulnerability());
    }

    private IEnumerator BlinkRoutine()
    {
        while (isInvulnerable)
        {
            if (playerRenderer != null)
                playerRenderer.enabled = !playerRenderer.enabled;

            yield return new WaitForSeconds(0.15f); // скорость мигания
        }

        // когда неуязвимость закончилась — включить обратно
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
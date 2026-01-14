using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;

    public bool IsDead { get; private set; } = false;

    [SerializeField] private SmerteKran smerteKran; // экран смерти

    private Collider playerCollider;

    private void Start()
    {
        currentHealth = maxHealth;
        playerCollider = GetComponent<Collider>();

        if (SaveSystem.HasCheckpoint())
        {
            transform.position = SaveSystem.LoadPosition();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable || IsDead) return;

        currentHealth -= amount;
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
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (IsDead) return;

        Debug.Log("Player died");
        IsDead = true;

        // Отключаем коллайдер, чтобы зона урона не мешала
        if (playerCollider != null)
            playerCollider.enabled = false;

        // Показ экрана смерти
        if (smerteKran != null)
            smerteKran.ShowGameOver();

       
        
           

       
        

        
        
    }

    private IEnumerator RespawnInvulnerability()
    {
        isInvulnerable = true;

        // Ждём чуть-чуть, чтобы UI успел отобразиться
        yield return new WaitForSeconds(0.1f);

        // Включаем коллайдер обратно
        if (playerCollider != null)
            playerCollider.enabled = true;

        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
        IsDead = false;
    }
    public void Respawn()
    {
        if (SaveSystem.HasCheckpoint())
            transform.position = SaveSystem.LoadPosition();

        currentHealth = maxHealth;
        IsDead = false;
        isInvulnerable = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        StartCoroutine(Invulnerability());
    }
}
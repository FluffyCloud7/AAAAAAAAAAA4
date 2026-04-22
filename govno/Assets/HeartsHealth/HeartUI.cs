using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public Image[] hearts;

    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    public PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth.OnHealthChanged += UpdateHearts;
        UpdateHearts(playerHealth.CurrentHealth, playerHealth.maxHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int currentHealth, int maxHealth)
    {
        int health = currentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (health >= 2)
            {
                hearts[i].sprite = fullHeart;
                health -= 2;
            }
            else if (health == 1)
            {
                hearts[i].sprite = halfHeart;
                health -= 1;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}
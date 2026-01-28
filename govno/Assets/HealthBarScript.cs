using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour

{
    public Slider HealthBarSlider;
    public PlayerHealth playerHealth;

    public int maxHealth;
    public int currHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerHealth.OnHealthChanged += UpdateHealthBar;
        HealthBarSlider.maxValue = playerHealth.maxHealth;
        HealthBarSlider.value = playerHealth.CurrentHealth;
    }

    private void OnDestroy()
    {
        // ќчень важно Ч отписатьс€!
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int current, int max)
    {
        HealthBarSlider.maxValue = max;
        HealthBarSlider.value = current;
    }
}

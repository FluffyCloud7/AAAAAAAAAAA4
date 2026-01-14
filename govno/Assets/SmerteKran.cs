using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SmerteKran : MonoBehaviour
{
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button mainMenuButton;

    private void Awake()
    {
        // Скрываем панель при старте
        gameOverPanel.SetActive(false);

        // Назначаем действия кнопок
        restartButton.onClick.AddListener(RestartLevel);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // пауза игры
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;

        PlayerHealth player = FindObjectOfType<PlayerHealth>();

        if (player != null)
            player.Respawn();
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

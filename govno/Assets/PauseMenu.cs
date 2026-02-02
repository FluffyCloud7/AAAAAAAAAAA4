using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public string mainMenuScene = "MainMenu";  // название сцены главного меню

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        //Time.timeScale = 1f;
        GamePauseManager.Instance.ReleasePause();
        CursorManager.Instance.SetMode(InputMode.Gameplay);
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        //Time.timeScale = 0f;
        GamePauseManager.Instance.RequestPause();
        CursorManager.Instance.SetMode(InputMode.UI);

        isPaused = true;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;  // важно вернуть время
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ReturnToCheckpoint()
    {
        // Возвращаем время, если пауза стояла
        Time.timeScale = 1f;

        // Респавним игрока
        respawnController.Instance.RespawnPlayer();

        // Закрываем меню
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

}
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public string mainMenuScene = "MainMenu";
    public PlayerHealth player;

    private bool isPaused = false;

    public Animator pauseAnimator;

    void Start()
    {
        // Меню начинается закрытым
        pauseAnimator.SetBool("IsOpen", false);
    }

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
        // Анимация закрытия
        pauseAnimator.SetBool("IsOpen", false);

        GamePauseManager.Instance.ReleasePause();
        CursorManager.Instance.SetMode(InputMode.Gameplay);

        isPaused = false;
    }

    void Pause()
    {
        // Анимация открытия
        pauseAnimator.SetBool("IsOpen", true);

        GamePauseManager.Instance.RequestPause();
        CursorManager.Instance.SetMode(InputMode.UI);

        isPaused = true;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ReturnToCheckpoint()
    {
        pauseAnimator.SetBool("IsOpen", false);

        GamePauseManager.Instance.ReleasePause();

        if (player != null)
            player.Respawn();

        CursorManager.Instance.SetMode(InputMode.Gameplay);

        isPaused = false;
    }
}
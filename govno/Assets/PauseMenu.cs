using System.Text;
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

    // НАШ НОВЫЙ МЕТОД ДЛЯ ВЫХОДА
    public void QuitGame()
    {
        // Этот код сработает в скомпилированном билде (.exe, .apk и т.д.)
        Application.Quit();

        // Этот код сработает ТОЛЬКО внутри редактора Unity, чтобы вы видели, что кнопка нажата
#if UNITY_EDITOR
        Encoding unityEditor = null; // Просто заглушка для компилятора, если нужно
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- Добавили для работы со слайдерами

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public string mainMenuScene = "MainMenu";
    public PlayerHealth player;

    [Header("Настройки аудио-ползунков")]
    public Slider musicSlider; // Привяжи сюда слайдер музыки в инспекторе
    public Slider sfxSlider;   // Привяжи сюда слайдер звуков в инспекторе

    private bool isPaused = false;

    public Animator pauseAnimator;

    void Start()
    {
        // Меню начинается закрытым
        pauseAnimator.SetBool("IsOpen", false);

        // Передаем ползунки в менеджеры и восстанавливаем сохраненные уровни громкости
        if (musicSlider != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.BindSlider(musicSlider);
        }

        if (sfxSlider != null && SoundEffectLibrary.Instance != null)
        {
            SoundEffectLibrary.Instance.BindSlider(sfxSlider);
        }
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
        Application.Quit();

#if UNITY_EDITOR
        Encoding unityEditor = null;
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
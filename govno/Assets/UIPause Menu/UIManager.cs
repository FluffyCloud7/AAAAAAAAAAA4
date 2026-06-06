using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Настройки UI")]
    [SerializeField] private GameObject hudPanel; // Панель со здоровьем
    [SerializeField] private GameObject pauseMenuPanel; // Панель паузы

    private bool isPaused = false;

    private void Awake()
    {
        // Делаем интерфейс бессмертным синглтоном
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Подписываемся на событие загрузки сцены, чтобы обновлять ссылки
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся от события при уничтожении
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Проверяем нажатие паузы (например, на Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Здесь мы лечим поломку связей!
        // Когда загрузилась новая сцена, интерфейсу нужно заново найти игрока
        UpdatePlayerReferences();
    }

    public void UpdatePlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[UIManager] Ссылки UI успешно обновлены для игрока: {player.name}");

            // ТУТ ТВОЙ КОД: Передай нового игрока в скрипт полоски здоровья
            // Например: healthBar.Initialize(player.GetComponent<Health>());
        }
        else
        {
            Debug.LogWarning("[UIManager] Игрок не найден на новой сцене для обновления UI!");
        }
    }

    public void TogglePause()
    {
        if (pauseMenuPanel == null) return;

        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);

        // Ставим игру на тайм-стоп, если открыта пауза
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
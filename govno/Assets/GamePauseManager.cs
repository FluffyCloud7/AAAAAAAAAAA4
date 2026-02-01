using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    private int pauseRequests;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RequestPause()
    {
        pauseRequests++;
        UpdatePauseState();
    }

    public void ReleasePause()
    {
        pauseRequests = Mathf.Max(0, pauseRequests - 1);
        UpdatePauseState();
    }

    private void UpdatePauseState()
    {
        IsPaused = pauseRequests > 0;
        Time.timeScale = IsPaused ? 0f : 1f;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public enum InputMode
{
    Gameplay,
    MouseGameplay,
    Dialogue,
    UI
}

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    private bool mouseModeAllowed = true;

    public void SetMouseModeAllowed(bool allowed)
    {
        mouseModeAllowed = allowed;

        if (!allowed && CurrentMode == InputMode.MouseGameplay)
        {
            SetMode(InputMode.Gameplay);
        }
    }

    public InputMode CurrentMode { get; private set; }

    private CinemachineVirtualCamera playerCam;
    private CinemachineVirtualCamera mouseCam;

    private const int ActivePriority = 20;
    private const int InactivePriority = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCamerasInScene();
        SetMode(CurrentMode); // восстанавливаем режим после загрузки
    }

    private void Start()
    {
        SetMode(InputMode.Gameplay);
    }

    private void FindCamerasInScene()
    {
        var cams = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (cam.name.Contains("Player"))
                playerCam = cam;

            if (cam.name.Contains("Mouse"))
                mouseCam = cam;
        }
    }


    public void SetMode(InputMode mode)
    {
        CurrentMode = mode;

        Debug.Log("Mode switched to: " + mode);

        switch (mode)
        {
            case InputMode.Gameplay:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                if (playerCam != null)
                    playerCam.Priority = ActivePriority;

                if (mouseCam != null)
                    mouseCam.Priority = InactivePriority;

                break;

            case InputMode.MouseGameplay:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (playerCam != null)
                    playerCam.Priority = InactivePriority;

                if (mouseCam != null)
                    mouseCam.Priority = ActivePriority;

                break;

            case InputMode.Dialogue:
            case InputMode.UI:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    private void Update()
    {
        if (!mouseModeAllowed)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (CurrentMode == InputMode.Gameplay)
                SetMode(InputMode.MouseGameplay);
            else if (CurrentMode == InputMode.MouseGameplay)
                SetMode(InputMode.Gameplay);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

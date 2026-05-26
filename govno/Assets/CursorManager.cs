using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

// Твой оригинальный enum, который используют все остальные системы в игре
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

    private CinemachineVirtualCameraBase playerCam;
    private CinemachineVirtualCameraBase mouseCam;

    private MonoBehaviour dynamicPlayerCam;
    private MonoBehaviour dynamicMouseCam;

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
        SetMode(CurrentMode);
    }

    private void Start()
    {
        SetMode(InputMode.Gameplay);
    }

    private void FindCamerasInScene()
    {
        dynamicPlayerCam = null;
        dynamicMouseCam = null;

        var allCams = FindObjectsByType<CinemachineVirtualCameraBase>(FindObjectsSortMode.None);

        foreach (var cam in allCams)
        {
            if (cam.name.Contains("Player"))
                dynamicPlayerCam = cam;

            if (cam.name.Contains("Mouse"))
                dynamicMouseCam = cam;
        }
    }

    public void SetMode(InputMode mode)
    {
        CurrentMode = mode;

        Debug.Log("Mode switched to: " + mode);

        TopDownPlayerMovement player = Object.FindFirstObjectByType<TopDownPlayerMovement>();
        Animator playerAnimator = null;
        if (player != null)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
        }

        switch (mode)
        {
            case InputMode.Gameplay:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked; // Запираем мышь по центру для управления персонажем

                SetCameraPriority(dynamicPlayerCam, ActivePriority);
                SetCameraPriority(dynamicMouseCam, InactivePriority);

                if (playerAnimator != null)
                    playerAnimator.SetBool("IsLookingUp", false);

                if (BirdFollowMouse.Instance != null)
                {
                    BirdFollowMouse.Instance.HideBird();
                }
                break;

            case InputMode.MouseGameplay:
                Cursor.visible = false; // СКРЫВАЕМ белую стрелку ОС
                Cursor.lockState = CursorLockMode.None; // Но разрешаем мыши двигаться, чтобы перемещать треугольник

                SetCameraPriority(dynamicPlayerCam, InactivePriority);
                SetCameraPriority(dynamicMouseCam, ActivePriority);

                if (playerAnimator != null)
                    playerAnimator.SetBool("IsLookingUp", true);

                if (BirdFollowMouse.Instance != null)
                {
                    BirdFollowMouse.Instance.ShowBird();
                }
                break;

            case InputMode.Dialogue:
            case InputMode.UI:
                Cursor.visible = false; // СКРЫВАЕМ белую стрелку ОС в диалогах и меню!
                Cursor.lockState = CursorLockMode.None;

                if (playerAnimator != null)
                    playerAnimator.SetBool("IsLookingUp", false);

                if (BirdFollowMouse.Instance != null)
                {
                    BirdFollowMouse.Instance.HideBird();
                }
                break;
        }
    }

    private void SetCameraPriority(MonoBehaviour cam, int priority)
    {
        if (cam == null) return;

        if (cam is CinemachineVirtualCamera vCam)
        {
            vCam.Priority = priority;
        }
        else if (cam is CinemachineFreeLook freeLookCam)
        {
            freeLookCam.Priority = priority;
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
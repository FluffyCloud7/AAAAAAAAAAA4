using UnityEngine;
using static UnityEngine.EventSystems.StandaloneInputModule;


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

    public InputMode CurrentMode { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
                break;

            case InputMode.MouseGameplay:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;

            case InputMode.Dialogue:
            case InputMode.UI:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }



    private void Start()
    {
        SetMode(InputMode.Gameplay);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (CurrentMode == InputMode.Gameplay)
            {
                SetMode(InputMode.MouseGameplay);
                Debug.Log("Mouse gameplay mode ON");
            }
            else if (CurrentMode == InputMode.MouseGameplay)
            {
                SetMode(InputMode.Gameplay);
                Debug.Log("Mouse gameplay mode OFF");
            }
        }
    }



}

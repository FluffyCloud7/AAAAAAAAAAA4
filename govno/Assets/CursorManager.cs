using UnityEngine;
using static UnityEngine.EventSystems.StandaloneInputModule;

public enum InputMode
{
    Gameplay,
    Dialogue,
    UI
}


public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

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
        switch (mode)
        {
            case InputMode.Gameplay:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
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
       CursorManager.Instance.SetMode(InputMode.Gameplay);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            bool isCursorVisible = Cursor.visible;

            if (isCursorVisible)
                CursorManager.Instance.SetMode(InputMode.Gameplay);
            else
                CursorManager.Instance.SetMode(InputMode.UI);
        }
    }

}

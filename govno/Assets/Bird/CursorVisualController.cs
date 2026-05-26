using UnityEngine;

public class CursorVisualController : MonoBehaviour
{
    [Header("Cursor Objects")]
    public GameObject triangleCursor;  // Твой UI-треугольник

    [Header("Movement")]
    public RectTransform cursorRoot;

    void Update()
    {
        if (CursorManager.Instance == null)
            return;

        UpdateCursorPosition();
        UpdateCursorVisual();
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot != null)
        {
            cursorRoot.position = Input.mousePosition;
        }
    }

    void UpdateCursorVisual()
    {
        // Кастуем режим к системному, чтобы гарантировать чтение твоего enum
        InputMode currentMode = (InputMode)CursorManager.Instance.CurrentMode;

        switch (currentMode)
        {
            case InputMode.Gameplay:
                // Когда бегаем в 3D — UI-треугольник выключен
                if (triangleCursor != null) triangleCursor.SetActive(false);
                break;

            case InputMode.MouseGameplay:
                // В режиме мыши на R (поскольку птицу убрали) — пусть летает треугольник
                if (triangleCursor != null) triangleCursor.SetActive(true);
                break;

            case InputMode.Dialogue:
            case InputMode.UI:
                // В диалогах и меню тоже горит треугольник
                if (triangleCursor != null) triangleCursor.SetActive(true);
                break;
        }
    }
}
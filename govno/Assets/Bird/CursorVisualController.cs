using UnityEngine;

public class CursorVisualController : MonoBehaviour
{
    [Header("Cursor Objects")]
    public GameObject birdCursor;
    public GameObject triangleCursor;

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
        cursorRoot.position = Input.mousePosition;
    }

    void UpdateCursorVisual()
    {
        switch (CursorManager.Instance.CurrentMode)
        {
            case InputMode.Gameplay:

                birdCursor.SetActive(false);
                triangleCursor.SetActive(false);

                break;

            case InputMode.MouseGameplay:

                birdCursor.SetActive(true);
                triangleCursor.SetActive(false);

                break;

            case InputMode.Dialogue:
            case InputMode.UI:

                birdCursor.SetActive(false);
                triangleCursor.SetActive(true);

                break;
        }
    }
}
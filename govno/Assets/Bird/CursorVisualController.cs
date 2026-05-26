using UnityEngine;
using UnityEngine.UI;

public class CursorVisualController : MonoBehaviour
{
    public static CursorVisualController Instance;

    [Header("Cursor Objects")]
    public GameObject triangleCursor;
    private Image triangleImage;

    [Header("Interactable Settings")]
    public Color normalColor = Color.white;
    public Color interactableColor = Color.purple; // Твой фиолетовый цвет из инспектора

    [Space]
    public Sprite normalSprite;
    public Sprite interactableSprite;

    [Header("Movement")]
    public RectTransform cursorRoot;

    private bool isHoveringInteractable = false;

    private void Awake()
    {
        Instance = this;
        if (triangleCursor != null)
        {
            triangleImage = triangleCursor.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (CursorManager.Instance == null)
            return;

        UpdateCursorPosition();
        UpdateCursorVisual();
    }

    // LateUpdate выполняется ПОСЛЕ того, как все объекты (бумага, губка, вырезы) посчитали свои лучи
    void LateUpdate()
    {
        // Сбрасываем флаг для следующего кадра
        isHoveringInteractable = false;
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot != null)
        {
            cursorRoot.position = Input.mousePosition;
        }
    }

    public void SetInteractableState(bool isOverInteractable)
    {
        // Если хотя бы один скрипт в этом кадре сказал true — флаг останется true
        if (isOverInteractable)
        {
            isHoveringInteractable = true;
        }
    }

    void UpdateCursorVisual()
    {
        InputMode currentMode = (InputMode)CursorManager.Instance.CurrentMode;

        switch (currentMode)
        {
            case InputMode.Gameplay:
                if (triangleCursor != null) triangleCursor.SetActive(false);
                break;

            case InputMode.MouseGameplay:
            case InputMode.Dialogue:
            case InputMode.UI:
                if (triangleCursor != null) triangleCursor.SetActive(true);

                ApplyInteractableVisual();
                break;
        }
    }

    private void ApplyInteractableVisual()
    {
        if (triangleImage == null) return;

        if (isHoveringInteractable)
        {
            triangleImage.color = interactableColor;
            if (interactableSprite != null) triangleImage.sprite = interactableSprite;
        }
        else
        {
            triangleImage.color = normalColor;
            if (normalSprite != null) triangleImage.sprite = normalSprite;
        }
    }
}
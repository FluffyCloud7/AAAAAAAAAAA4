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
    public Color interactableColor = Color.purple;

    [Space]
    public Sprite normalSprite;
    public Sprite interactableSprite;

    [Header("Movement")]
    public RectTransform cursorRoot;

    [Header("Raycast Setup")]
    public LayerMask interactableLayers; // Сюда в инспекторе выбери слои бумаги, губки и т.д.
    public float rayDistance = 100f;

    private bool isHoveringInteractable = false;
    private Camera mainCam;

    private void Awake()
    {
        Instance = this;
        if (triangleCursor != null)
        {
            triangleImage = triangleCursor.GetComponent<Image>();
        }
        mainCam = Camera.main;
    }

    void Update()
    {
        if (CursorManager.Instance == null)
            return;

        UpdateCursorPosition();
        CheckForInteractableObjects(); // Сам проверяет, наведены ли мы на что-то
        UpdateCursorVisual();
    }

    void UpdateCursorPosition()
    {
        if (cursorRoot != null)
        {
            cursorRoot.position = Input.mousePosition;
        }
    }

    private void CheckForInteractableObjects()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        // Пускаем луч из камеры через позицию мыши
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayers))
        {
            // Проверяем, есть ли на объекте скрипт губки или бумаги или общий маркер интерактивности
            bool hasInteractable = hit.transform.GetComponent<InteractableObject>() != null ||
                                   hit.transform.GetComponent<RigidPageCurl>() != null ||
                                   hit.transform.GetComponent<SpongeCleaner>() != null;

            if (hasInteractable)
            {
                isHoveringInteractable = true;
                return;
            }
        }

        // Если луч никуда не попал или попал не туда — выключаем подсветку
        isHoveringInteractable = false;
    }

    // Этот метод можно оставить для экстренных случаев (например, во время перетаскивания)
    public void SetInteractableState(bool isOverInteractable)
    {
        isHoveringInteractable = isOverInteractable;
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
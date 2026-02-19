using UnityEngine;
using UnityEngine.EventSystems;

public class InputModeDetector : MonoBehaviour
{
    public bool IsMouseGameplayActive { get; private set; }
    public bool IsKeyboardActive { get; private set; }

    void Update()
    {
        // Проверяем WASD
        IsKeyboardActive =
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        // Проверяем движение мыши
        bool mouseMoved =
            Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f;

        // Не считаем мышь если курсор над UI
        bool overUI = EventSystem.current != null &&
                      EventSystem.current.IsPointerOverGameObject();

        IsMouseGameplayActive = mouseMoved && !overUI;
    }
}

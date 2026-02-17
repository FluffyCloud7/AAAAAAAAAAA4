using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera cam;

    Rigidbody rb;
    DefaultInputActions input;

    Vector2 move;      // ← сюда читаем input system
    Vector3 moveInput; // ← это твой старый формат

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        input = new DefaultInputActions();
    }

    void OnEnable()
    {
        input.Player.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= OnMove;

        input.Player.Disable();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();

        // 🔽 ВОТ ЗДЕСЬ происходит конвертация
        moveInput = new Vector3(move.x, 0f, move.y);
    }

    void FixedUpdate()
    {
        // Движение относительно камеры
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;

        Vector3 camRight = cam.transform.right;
        camRight.y = 0;

        Vector3 moveDir =
            (camForward * moveInput.z + camRight * moveInput.x).normalized;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        if (moveDir != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(moveDir));
    }
}

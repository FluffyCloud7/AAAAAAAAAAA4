using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera cam;

    Rigidbody rb;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // PlayerInput вызовет это автоматически
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (CursorManager.Instance.CurrentMode != InputMode.Gameplay)
            return;

        Vector3 moveDir = new Vector3(move.x, 0, move.y).normalized;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                10f * Time.fixedDeltaTime
            ));
        }
    }

}

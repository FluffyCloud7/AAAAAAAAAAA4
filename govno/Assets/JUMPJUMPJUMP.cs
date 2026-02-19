using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 6f;
    public int maxJumps = 2;
    public LayerMask groundMask;

    Rigidbody rb;
    int jumpsLeft;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpsLeft = maxJumps;
    }

    // вызывается PlayerInput автоматически
    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (IsGrounded())
            jumpsLeft = maxJumps;

        if (jumpsLeft > 0)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsLeft--;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            1.1f,
            groundMask
        );
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    public float jumpHeight = 3.2f;
    public int maxJumps = 1;

    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    Animator animator;

    CharacterController controller;
    TopDownPlayerMovement movement;

    int jumpsLeft;

    float coyoteCounter;
    float jumpBufferCounter;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<TopDownPlayerMovement>();
        jumpsLeft = maxJumps;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ---- COYOTE TIME ----
        if (controller.isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // ---- JUMP BUFFER ----
        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            PerformJump();
            jumpBufferCounter = 0f;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    void PerformJump()
    {
        if (controller.isGrounded)
            jumpsLeft = maxJumps;

        if (jumpsLeft > 0)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * movement.gravity);

            movement.SetVerticalVelocity(jumpVelocity);

            animator.SetTrigger("Jump");

            jumpsLeft--;
            coyoteCounter = 0f;
        }
    }
}
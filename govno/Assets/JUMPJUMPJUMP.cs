using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerJump : MonoBehaviour
{
    public float jumpHeight = 2f;
    public int maxJumps = 1;

    Animator animator;

    CharacterController controller;
    TopDownPlayerMovement movement;

    int jumpsLeft;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<TopDownPlayerMovement>();
        jumpsLeft = maxJumps;
        animator = GetComponentInChildren<Animator>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;

        if (controller.isGrounded)
            jumpsLeft = maxJumps;

        if (jumpsLeft > 0)
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * movement.gravity);
            movement.SetVerticalVelocity(jumpVelocity);
            animator.SetTrigger("Jump");
            jumpsLeft--;
        }
    }
}
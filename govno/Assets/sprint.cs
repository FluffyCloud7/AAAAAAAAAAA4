using UnityEngine;
using UnityEngine.InputSystem;

public class SprintController : MonoBehaviour
{
    public TopDownPlayerMovement movement;
    public float sprintMultiplier = 1.8f;

    float baseSpeed;

    void Awake()
    {
        if (movement == null)
            movement = GetComponent<TopDownPlayerMovement>();

        baseSpeed = movement.moveSpeed;
    }

    void Update()
    {
        bool sprintHeld = Keyboard.current.leftShiftKey.isPressed;

        movement.moveSpeed = sprintHeld
            ? baseSpeed * sprintMultiplier
            : baseSpeed;
    }
}

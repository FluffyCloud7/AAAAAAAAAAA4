using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;

    CharacterController controller;
    Vector2 move;
    float verticalVelocity;

    private MovingPlatform currentPlatform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void SetVerticalVelocity(float value)
    {
        verticalVelocity = value;
    }

    void Update()
    {
        if (CursorManager.Instance.CurrentMode != InputMode.Gameplay)
            return;

        Vector3 moveDir = new Vector3(move.x, 0, move.y).normalized;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = verticalVelocity;

        if (controller.isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                currentPlatform = hit.collider.GetComponentInParent<MovingPlatform>();
            }
        }
        else
        {
            currentPlatform = null;
        }

        Vector3 platformMovement = Vector3.zero;

        if (currentPlatform != null)
        {
            platformMovement = currentPlatform.DeltaMovement;
        }

        controller.Move(velocity * Time.deltaTime + platformMovement);

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
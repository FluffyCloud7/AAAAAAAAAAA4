using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float rotationSpeed = 12f;

    public float gravity = -30f;              // сильнее обычной
    public float fallMultiplier = 2f;         // ускоренное падение
    public float acceleration = 60f;          // разгон
    public float deceleration = 70f;          // торможение
    public float airControl = 0.6f;           // контроль в воздухе

    Animator animator;

    CharacterController controller;
    Vector2 move;
    float verticalVelocity;

    private MovingPlatform currentPlatform;

    Vector3 currentHorizontalVelocity;        // НОВОЕ

    // Ссылка на скрипт здоровья для проверки состояния смерти
    private PlayerHealth playerHealth;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Безопасно ищем компонент здоровья на этом же объекте игрока
        playerHealth = GetComponent<PlayerHealth>();
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
        // Игрок может управлять, только если геймплейный режим И персонаж не мертв
        bool isDead = playerHealth != null && playerHealth.IsDead;
        bool canControl = CursorManager.Instance.CurrentMode == InputMode.Gameplay && !isDead;

        Vector3 inputDir = Vector3.zero;

        if (canControl)
            inputDir = new Vector3(move.x, 0, move.y).normalized;

        // ---------- ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ (ПЛАВНОЕ) ----------

        float control = controller.isGrounded ? 1f : airControl;

        Vector3 targetVelocity = inputDir * moveSpeed * control;

        if (inputDir != Vector3.zero)
        {
            currentHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            float decel = controller.isGrounded ? deceleration : deceleration * 0.3f;

            currentHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                Vector3.zero,
                decel * Time.deltaTime
            );
        }

        // ---------- ГРАВИТАЦИЯ ----------

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        // разная гравитация вверх и вниз
        if (verticalVelocity < 0)
            verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
        else
            verticalVelocity += gravity * Time.deltaTime;

        // ---------- ПЛАТФОРМА ----------

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
            platformMovement = currentPlatform.DeltaMovement;

        // ---------- MOVE ----------

        Vector3 finalVelocity = currentHorizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime + platformMovement);

        // ---------- ПОВОРОТ ----------

        if (canControl && inputDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        animator.SetFloat("Speed", canControl ? currentHorizontalVelocity.magnitude : 0f);
        animator.SetBool("IsGrounded", controller.isGrounded);
    }
}
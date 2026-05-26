using Cinemachine;
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

    Vector3 currentHorizontalVelocity;

    // Ссылка на скрипт здоровья для проверки состояния смерти
    private PlayerHealth playerHealth;

    // Ссылка на главную камеру для вычисления направления движения
    private Transform mainCameraTransform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();

        // Находим главную камеру на сцене при старте
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
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

        // Расчет направления движения относительно камеры (Идеальный вариант для 3D платформиров)
        if (canControl && move != Vector2.zero && mainCameraTransform != null)
        {
            // Берем чистые направления камеры
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;

            // ЖЕСТКО обнуляем Y составляющую, чтобы наклон камеры вообще не влиял на WASD
            camForward.y = 0f;
            camRight.y = 0f;

            // Нормализуем очищенные от вертикали векторы
            camForward.Normalize();
            camRight.Normalize();

            // Считаем итоговое направление движения в горизонтальной проекции
            inputDir = (camForward * move.y + camRight * move.x).normalized;
        }

        // ---------- ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ (ПЛАВНОЕ) ----------

        float control = controller.isGrounded ? 1f : airControl;

        Vector3 targetVelocity = (move != Vector2.zero && canControl) ? inputDir * moveSpeed * control : Vector3.zero;

        if (move != Vector2.zero && canControl)
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

        if (canControl && move != Vector2.zero && inputDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // ---------- АНИМАЦИЯ ----------

        float animationSpeedValue = 0f;

        if (canControl && move != Vector2.zero)
        {
            // Берем реальную скорость, но математически округляем её до 2 знаков после запятой,
            // чтобы отрезать микро-колебания от вращения камеры, которые ломали аниматор.
            animationSpeedValue = Mathf.Round(currentHorizontalVelocity.magnitude * 100f) / 100f;
        }

        // Передаем честное и стабильное значение скорости
        animator.SetFloat("Speed", animationSpeedValue);
        animator.SetBool("IsGrounded", controller.isGrounded);


        // ---------- ЧЕСТНЫЙ АВТО-ВОЗВРАТ КАМЕРЫ КОДОМ ----------
        if (mainCameraTransform != null)
        {
            CinemachineFreeLook freeLook = mainCameraTransform.GetComponentInParent<CinemachineFreeLook>();

            if (freeLook != null)
            {
                bool isMoving = move != Vector2.zero && canControl;
                float mouseX = Mouse.current.delta.x.ReadValue();

                // Если игрок бежит, но не трогает мышь
                if (isMoving && Mathf.Abs(mouseX) < 0.01f)
                {
                    // Считаем разницу между углом персонажа (его затылком) и текущим углом камеры
                    // Вычитаем 180, так как моделька в Блендере настроена по Y
                    float targetHeading = transform.eulerAngles.y - 180f;
                    float currentHeading = freeLook.m_XAxis.Value;

                    // Вычисляем кратчайший путь поворота
                    float deltaAngle = Mathf.DeltaAngle(currentHeading, targetHeading);

                    // ТЕСТОВЫЙ ЛОГ: Выводит углы в консоль для проверки работы
                    // Debug.Log($"Cam: {currentHeading:F1}, Target: {targetHeading:F1}, Delta: {deltaAngle:F1}");

                    // Имитируем искусственный "ввод мыши" прямо в ось, обходя блокировку Cinemachine
                    freeLook.m_XAxis.m_InputAxisValue = Mathf.Lerp(0f, deltaAngle, 1.5f * Time.deltaTime);
                }
                else
                {
                    // Если игрок тронул мышь — полностью возвращаем управление ему
                    freeLook.m_XAxis.m_InputAxisValue = mouseX;
                }
            }
        }
    }
}
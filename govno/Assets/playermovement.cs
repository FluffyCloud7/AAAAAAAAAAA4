using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float rotationSpeed = 12f;

    public float gravity = -30f;
    public float fallMultiplier = 2f;
    public float acceleration = 60f;
    public float deceleration = 70f;
    public float airControl = 0.6f;

    [Header("Настройки звуков шагов")]
    [SerializeField] private string footstepSoundGroup = "Footsteps";
    [SerializeField] private float pitchRandomness = 0.15f; // Случайное изменение тональности (15%)
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.4f;   // Настройка громкости шагов (0.4 = 40%)

    Animator animator;
    CharacterController controller;
    Vector2 moveInput;
    float verticalVelocity;

    private MovingPlatform currentPlatform;
    Vector3 currentHorizontalVelocity;
    Vector3 inputDir;

    private PlayerHealth playerHealth;
    private Transform mainCameraTransform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (Camera.main != null) mainCameraTransform = Camera.main.transform;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void SetVerticalVelocity(float value)
    {
        verticalVelocity = value;
    }

    // Этот метод вызывается через наш скрипт-передатчик из кадров анимации ходьбы и бега
    public void PlayFootstep()
    {
        // Звук срабатывает только если персонаж на земле и имеет скорость
        if (controller.isGrounded && currentHorizontalVelocity.magnitude > 0.1f)
        {
            if (!string.IsNullOrEmpty(footstepSoundGroup))
            {
                // Считаем рандомный питч вокруг единицы
                float randomPitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);

                // Передаем имя группы, питч и громкость шагов четвертым параметром
                SoundEffectManager.PlayVoice(footstepSoundGroup, randomPitch, false, footstepVolume);
            }
        }
    }

    void Update()
    {
        bool isDead = playerHealth != null && playerHealth.IsDead;
        bool canControl = CursorManager.Instance.CurrentMode == InputMode.Gameplay && !isDead;

        if (canControl && moveInput != Vector2.zero && mainCameraTransform != null)
        {
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            inputDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        }
        else
        {
            inputDir = Vector3.zero;
        }

        if (canControl && moveInput != Vector2.zero && inputDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float animationSpeedValue = 0f;
        if (canControl && moveInput != Vector2.zero)
        {
            animationSpeedValue = Mathf.Round(currentHorizontalVelocity.magnitude * 100f) / 100f;
        }
        animator.SetFloat("Speed", animationSpeedValue);
        animator.SetBool("IsGrounded", controller.isGrounded);

        if (mainCameraTransform != null)
        {
            CinemachineFreeLook freeLook = mainCameraTransform.GetComponentInParent<CinemachineFreeLook>();
            if (freeLook != null)
            {
                bool isMoving = moveInput != Vector2.zero && canControl;
                float mouseX = Mouse.current.delta.x.ReadValue();

                if (isMoving && Mathf.Abs(mouseX) < 0.01f)
                {
                    float targetHeading = transform.eulerAngles.y - 180f;
                    float currentHeading = freeLook.m_XAxis.Value;
                    float deltaAngle = Mathf.DeltaAngle(currentHeading, targetHeading);
                    freeLook.m_XAxis.m_InputAxisValue = Mathf.Lerp(0f, deltaAngle, 1.5f * Time.deltaTime);
                }
                else
                {
                    freeLook.m_XAxis.m_InputAxisValue = mouseX;
                }
            }
        }
    }

    void FixedUpdate()
    {
        bool isDead = playerHealth != null && playerHealth.IsDead;
        bool canControl = CursorManager.Instance.CurrentMode == InputMode.Gameplay && !isDead;

        // ---------- ГОРИЗОНТАЛЬНОЕ ДВИЖЕНИЕ ----------
        float control = controller.isGrounded ? 1f : airControl;
        Vector3 targetVelocity = (moveInput != Vector2.zero && canControl) ? inputDir * moveSpeed * control : Vector3.zero;

        if (moveInput != Vector2.zero && canControl)
        {
            currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            float decel = controller.isGrounded ? deceleration : deceleration * 0.3f;
            currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, Vector3.zero, decel * Time.fixedDeltaTime);
        }

        // ---------- ГРАВИТАЦИЯ ----------
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -4f;

        if (verticalVelocity < 0)
            verticalVelocity += gravity * fallMultiplier * Time.fixedDeltaTime;
        else
            verticalVelocity += gravity * Time.fixedDeltaTime;

        // ---------- ОБНАРУЖЕНИЕ ПЛАТФОРМЫ ----------
        RaycastHit hit;
        float rayLength = (controller.height * 0.5f) + 0.5f;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength))
        {
            currentPlatform = hit.collider.GetComponentInParent<MovingPlatform>();
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

        // ---------- ФИНАЛЬНОЕ ПЕРЕМЕЩЕНИЕ ----------
        Vector3 finalVelocity = currentHorizontalVelocity;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.fixedDeltaTime);

        if (currentPlatform != null)
        {
            controller.enabled = false;
            transform.position += platformMovement;
            controller.enabled = true;
        }
    }
}
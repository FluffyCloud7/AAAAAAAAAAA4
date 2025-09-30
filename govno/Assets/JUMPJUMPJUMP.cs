using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;

    [Header("Ground check")]
    [Tooltip("От центра вверх, чтобы луч не попадал в собственный коллайдер")]
    [SerializeField] private float originOffsetUp = 0.1f;
    [Tooltip("Расстояние от origin до земли (пример для куба высотой 1 = 0.6)")]
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundMask = 1 << 0; // Default по умолчанию

    [Header("Safety")]
    [SerializeField] private float jumpCooldown = 0.08f; // защита от двойных срабатываний

    private Rigidbody rb;
    private int jumpsLeft;
    private bool jumpRequested;
    private float lastJumpTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jumpsLeft = maxJumps;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // Буфер запроса прыжка — только когда нажали (один кадр)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
            Debug.Log("[Jump] Request received");
        }
    }

    private void FixedUpdate()
    {
        // Надёжная проверка земли:
        // ставим источник чуть выше центра, чтобы не попасть в собственный коллайдер.
        Vector3 origin = transform.position + Vector3.up * originOffsetUp;
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        // Игнорируем попадание в самого себя на всякий случай
        bool isGrounded = hitSomething && hit.collider != null && hit.collider.gameObject != gameObject;

        // Если только что приземлились — восстановить прыжки
        // (мы не делаем reset в Update, чтобы избежать гонок с физикой)
        if (isGrounded)
        {
            // нормальная ситуация — стоим на земле
            jumpsLeft = maxJumps;
        }

        // Выполняем прыжок только если был запрос и прошёл cooldown
        if (jumpRequested)
        {
            jumpRequested = false; // потребили запрос

            if (Time.time - lastJumpTime < jumpCooldown)
            {
                // слишком рано — игнорируем
                Debug.Log("[Jump] Ignored due to cooldown");
            }
            else if (jumpsLeft > 0)
            {
                // Прыгаем
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // обнуляем Y перед импульсом
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpsLeft--;
                lastJumpTime = Time.time;
                Debug.Log($"[Jump] Performed. Jumps left: {jumpsLeft}");
            }
            else
            {
                Debug.Log("[Jump] Request but no jumps left");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // визуализация луча для отладки
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * originOffsetUp;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
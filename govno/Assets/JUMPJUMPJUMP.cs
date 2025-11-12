using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    [Header("Jump settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;

    [Header("Ground check")]
    [Tooltip("�� ������ �����, ����� ��� �� ������� � ����������� ���������")]
    [SerializeField] private float originOffsetUp = 0.1f;
    [Tooltip("���������� �� origin �� ����� (������ ��� ���� ������� 1 = 0.6)")]
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundMask = 1 << 0; // Default �� ���������

    [Header("Safety")]
    [SerializeField] private float jumpCooldown = 0.08f; // ������ �� ������� ������������

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
        // ����� ������� ������ � ������ ����� ������ (���� ����)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
            Debug.Log("[Jump] Request received");
        }
    }

    private void FixedUpdate()
    {
        // ������� �������� �����:
        // ������ �������� ���� ���� ������, ����� �� ������� � ����������� ���������.
        Vector3 origin = transform.position + Vector3.up * originOffsetUp;
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        // ���������� ��������� � ������ ���� �� ������ ������
        bool isGrounded = hitSomething && hit.collider != null && hit.collider.gameObject != gameObject;

        // ���� ������ ��� ������������ � ������������ ������
        // (�� �� ������ reset � Update, ����� �������� ����� � �������)
        if (isGrounded)
        {
            // ���������� �������� � ����� �� �����
            jumpsLeft = maxJumps;
        }

        // ��������� ������ ������ ���� ��� ������ � ������ cooldown
        if (jumpRequested)
        {
            jumpRequested = false; // ��������� ������

            if (Time.time - lastJumpTime < jumpCooldown)
            {
                // ������� ���� � ����������
                Debug.Log("[Jump] Ignored due to cooldown");
            }
            else if (jumpsLeft > 0)
            {
                // �������
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // �������� Y ����� ���������
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
        // ������������ ���� ��� �������
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * originOffsetUp;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
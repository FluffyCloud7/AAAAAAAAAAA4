using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TopDownPlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera cam;
    Rigidbody rb;

    Vector3 moveInput; // сохраняем ввод между кадрами

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // на всякий случай
    }

    void Update()
    {
        // Получаем ввод (только тут!)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        // Движение относительно камеры
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        Vector3 camRight = cam.transform.right;
        camRight.y = 0;

        Vector3 moveDir = (camForward * moveInput.z + camRight * moveInput.x).normalized;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // Поворот в сторону движения
        if (moveDir != Vector3.zero)
            rb.MoveRotation(Quaternion.LookRotation(moveDir));
    }
}
using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -10);
    public Rigidbody targetRb;


    public float smoothTime = 0.1f;
    private Vector3 velocity;

    void FixedUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = targetRb.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // фиксируем угол, не используем LookAt!
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
}
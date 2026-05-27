using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 movementDirection = Vector3.up;
    [SerializeField] private float distance = 4f;
    [SerializeField] private float speed = 2f;

    private Rigidbody rb;
    private Vector3 startPosition;

    // Считаем позицию глобально
    public Vector3 TargetPosition { get; private set; }
    public Vector3 DeltaMovement { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;

        startPosition = transform.position;
        TargetPosition = transform.position;
        movementDirection.Normalize();
    }

    // Изменили на LateFixedUpdate (образно), считаем позицию прямо перед движением физики
    void FixedUpdate()
    {
        float factor = Mathf.PingPong(Time.fixedTime * speed, distance);
        Vector3 nextPosition = startPosition + movementDirection * factor;

        // Вычисляем дельту движения ДО того, как физически переместить объект
        DeltaMovement = nextPosition - transform.position;

        // Запоминаем, куда идет платформа
        TargetPosition = nextPosition;

        // Двигаем платформу
        rb.MovePosition(TargetPosition);
    }
}
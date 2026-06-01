using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Направление движения в локальных координатах платформы")]
    [SerializeField] private Vector3 movementDirection = Vector3.up;
    [SerializeField] private float distance = 4f;
    [SerializeField] private float speed = 2f;

    [Header("Editor Visualization")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private bool showAlways = false;

    private Rigidbody rb;
    private Vector3 startPosition;

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

        // Переводим направление в локальное пространство при старте, если нужно,
        // но для простоты нормализуем базовый вектор
        movementDirection.Normalize();
    }

    void FixedUpdate()
    {
        float factor = Mathf.PingPong(Time.fixedTime * speed, distance);

        // Рассчитываем целевую позицию относительно стартовой
        // Используем transform.TransformDirection, чтобы направление учитывало поворот платформы
        Vector3 globalDirection = transform.TransformDirection(movementDirection).normalized;
        Vector3 nextPosition = startPosition + globalDirection * factor;

        DeltaMovement = nextPosition - transform.position;
        TargetPosition = nextPosition;

        rb.MovePosition(TargetPosition);
    }

    // --- Отрисовка Гизмос для удобства Левел-Дизайна ---

    private void OnDrawGizmos()
    {
        if (showAlways) DrawPlatformPath();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAlways) DrawPlatformPath();
    }

    private void DrawPlatformPath()
    {
        // Пока игра не запущена, берем текущую позицию за стартовую
        Vector3 origin = Application.isPlaying ? startPosition : transform.position;

        // Учитываем поворот объекта для корректного отображения локального направления
        Vector3 globalDirection = transform.TransformDirection(movementDirection).normalized;
        Vector3 endPosition = origin + globalDirection * distance;

        // Рисуем линию движения
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(origin, endPosition);

        // Рисуем начальную точку (сфера)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, 0.2f);

        // Рисуем конечную точку (сфера)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPosition, 0.2f);

        // Опционально: можно нарисовать контур платформы в конечной точке, 
        // если на объекте есть BoxCollider или MeshFilter, но сферы обычно достаточно.
    }
}
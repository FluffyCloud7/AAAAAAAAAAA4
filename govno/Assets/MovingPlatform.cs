using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private Vector3[] localPoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool pingPong = false;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTimeAtPoint = 0f;

    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer;
    private Vector3[] worldPoints;

    private void Start()
    {
        worldPoints = new Vector3[localPoints.Length];
        for (int i = 0; i < localPoints.Length; i++)
        {
            worldPoints[i] = transform.TransformPoint(localPoints[i]);
        }
    }

    private void Update()
    {
        if (worldPoints.Length < 2)
            return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 target = worldPoints[currentIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            waitTimer = waitTimeAtPoint;
            NextPoint();
        }
    }

    private void NextPoint()
    {
        currentIndex += direction;

        if (pingPong)
        {
            if (currentIndex >= worldPoints.Length || currentIndex < 0)
            {
                direction *= -1;
                currentIndex += direction * 2;
            }
        }
        else if (loop)
        {
            if (currentIndex >= worldPoints.Length)
                currentIndex = 0;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (localPoints == null || localPoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < localPoints.Length; i++)
        {
            Vector3 world = transform.TransformPoint(localPoints[i]);
            Gizmos.DrawSphere(world, 0.2f);

            if (i < localPoints.Length - 1)
            {
                Vector3 next = transform.TransformPoint(localPoints[i + 1]);
                Gizmos.DrawLine(world, next);
            }
        }
    }
#endif
}
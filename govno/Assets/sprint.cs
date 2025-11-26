using UnityEngine;

public class SprintController : MonoBehaviour
{
    public TopDownPlayerMovement movement; // ссылка на твой скрипт передвижения
    public float sprintMultiplier = 1.8f;  // во сколько раз быстрее при спринте

    float baseSpeed;

    void Start()
    {
        if (movement == null)
            movement = GetComponent<TopDownPlayerMovement>();

        baseSpeed = movement.moveSpeed;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement.moveSpeed = baseSpeed * sprintMultiplier;
        }
        else
        {
            movement.moveSpeed = baseSpeed;
        }
    }
}

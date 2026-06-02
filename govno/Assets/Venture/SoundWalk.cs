using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private TopDownPlayerMovement movement;

    void Awake()
    {
        // Ищем скрипт движения на родительском объекте
        movement = GetComponentInParent<TopDownPlayerMovement>();
    }

    // Этот метод теперь будет виден Аниматору!
    public void PlayFootstep()
    {
        if (movement != null)
        {
            movement.PlayFootstep(); // Перенаправляем вызов в главный скрипт движения
        }
    }
}
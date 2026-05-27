using UnityEngine;

public class CharacterPush : MonoBehaviour
{
    // Сила, с которой персонаж будет пинать объекты
    public float pushPower = 2.0f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Если у объекта нет Rigidbody или он Kinematic — игнорируем
        if (body == null || body.isKinematic)
        {
            return;
        }

        // Не толкаем объекты, которые находятся строго под нами
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // Вычисляем направление толчка на основе движения персонажа
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Прикладываем силу к объекту в точке удара
        body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
    }
}
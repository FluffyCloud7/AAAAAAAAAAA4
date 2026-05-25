using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что в бездну упал именно игрок
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Вызываем мгновенную смерть для бездны
                playerHealth.TakeVoidDamage();
            }
        }
    }
}
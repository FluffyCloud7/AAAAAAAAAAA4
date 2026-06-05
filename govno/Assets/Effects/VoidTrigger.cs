using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Проверяем, что в бездну упал именно игрок
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Вызываем мгновенную смерть для бездны
                playerHealth.TakeVoidDamage();
            }
            return; // Выходим из метода, так как это был игрок
        }

        // 2. Если это не игрок, проверяем, не губка ли это упала
        SpongeCleaner sponge = other.GetComponent<SpongeCleaner>();
        if (sponge != null)
        {
            // Возвращаем губку на стартовую позицию и моем её
            sponge.RespawnSponge();
            Debug.Log("[VoidTrigger]: Губка упала в бездну и была возвращена на спавн.");
        }
    }
}
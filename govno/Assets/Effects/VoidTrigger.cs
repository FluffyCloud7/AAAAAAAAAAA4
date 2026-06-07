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
                playerHealth.TakeVoidDamage();
            }
            return;
        }

        // 2. Проверяем, не губка ли это упала
        SpongeCleaner sponge = other.GetComponent<SpongeCleaner>();
        if (sponge != null)
        {
            sponge.RespawnSponge();
            Debug.Log("[VoidTrigger]: Губка упала в бездну и была возвращена на спавн.");
            return; // Выходим, чтобы не делать лишних проверок
        }

        // 3. ПРОВЕРКА НА ТЯЖЕЛЫЙ ПРЕДМЕТ
        GrabbableItem item = other.GetComponent<GrabbableItem>();
        if (item != null)
        {
            // Проверяем, что предмет именно тяжелый
            if (item.itemSize == ItemSize.HeavyInHands)
            {
                item.Respawn();
                Debug.Log($"[VoidTrigger]: Тяжелый предмет {item.itemName} возвращен на спавн.");
            }
        }
    }
}
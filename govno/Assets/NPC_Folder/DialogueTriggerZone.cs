using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [Header("Ссылка на NPC, чей диалог запускаем")]
    [SerializeField] private NPC targetNPC;

    [Header("Удалять триггер после первого пересечения?")]
    [SerializeField] private bool triggerOnlyOnce = true;

    [Header("Тег игрока для проверки")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что вошел именно игрок, диалог еще не активен и триггер не отработал свое
        if (other.CompareTag(playerTag) && !hasTriggered && targetNPC != null)
        {
            // Запускаем диалог точно так же, как если бы нажали 'E'
            targetNPC.Interact();

            if (triggerOnlyOnce)
            {
                hasTriggered = true;
                // Можно выключить коллайдер или сам объект, если он больше не нужен
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    // Если игра 2D, просто замени OnTriggerEnter на OnTriggerEnter2D
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasTriggered && targetNPC != null)
        {
            targetNPC.Interact();
            if (triggerOnlyOnce)
            {
                hasTriggered = true;
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
    */
}
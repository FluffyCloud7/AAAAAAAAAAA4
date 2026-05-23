using UnityEngine;

public class HintTriggerZone : MonoBehaviour
{
    [Header("Ссылка на подсказку")]
    [Tooltip("Перетащи сюда объект со стены, на котором висит скрипт HintController")]
    public HintController hintController;

    [Header("Настройки фильтра")]
    public string playerTag = "Player";

    // Предохранитель вместо грубого отключения компонента
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Если уже активировано или зашел не игрок — игнорируем
        if (isActivated || !other.CompareTag(playerTag)) return;

        if (hintController != null)
        {
            isActivated = true; // Блокируем повторные вызовы
            hintController.TriggerActivation();
        }
        else
        {
            Debug.LogWarning($"[HintTriggerZone] На объекте {gameObject.name} не назначена ссылка на HintController!");
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawCube(transform.position + box.center, box.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }

        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class LevelDoor : MonoBehaviour
{
    [Header("Идентификация двери")]
    public string doorID;

    [Header("Куда ведет дверь")]
    public string targetSceneName;
    public string targetDoorID;

    [Header("Точка появления")]
    public Transform spawnPoint;

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && (health.CurrentHealth <= 0 || health.IsDead)) return;

            isTransitioning = true;
            Debug.Log($"[LevelDoor] Начинается переход...");

            // Запускаем переход через менеджер
            PerformTransition(other.gameObject);
        }
    }

    private void PerformTransition(GameObject player)
    {
        // КРИТИЧЕСКИ ВАЖНО (ХАК): Сбрасываем точку спавна у синглтона. 
        // Если онDontDestroyOnLoad, это сработает.
        if (respawnController.Instance != null)
        {
            respawnController.Instance.respawnPoint = null;
            Debug.Log("[LevelDoor] ХАК: Точка спавна у синглтона сброшена в NULL перед переходом.");
        }

        List<GameObject> objectsToPreserve = new List<GameObject>();
        objectsToPreserve.Add(player);

        // Мы не переносим камеры здесь, менеджер их найдет
        TransitionManager.Instance.TargetTransition(targetSceneName, targetDoorID, player, objectsToPreserve);
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(spawnPoint.position, 0.2f);
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 1f);
        }
    }
}
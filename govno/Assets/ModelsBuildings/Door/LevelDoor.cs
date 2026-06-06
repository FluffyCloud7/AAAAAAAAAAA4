using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            // Проверка на смерть
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && (health.CurrentHealth <= 0 || health.IsDead)) return;

            isTransitioning = true;
            StartCoroutine(StableTransitionRoutine(other.gameObject));
        }
    }

    private IEnumerator StableTransitionRoutine(GameObject player)
    {
        if (respawnController.Instance != null)
        {
            respawnController.Instance.respawnPoint = spawnPoint;
        }

        yield return null;

        // СОЗДАЕМ СПИСОК ДЛЯ ПЕРЕДАЧИ В MANAGER
        List<GameObject> objectsToPreserve = new List<GameObject>();
        objectsToPreserve.Add(player);

        if (TransitionManager.Instance != null)
        {
            // ПЕРЕДАЕМ ВСЕ 4 АРГУМЕНТА
            TransitionManager.Instance.TargetTransition(targetSceneName, targetDoorID, player, objectsToPreserve);
        }
        else
        {
            Debug.LogError("[LevelDoor] TransitionManager не найден!");
        }
    }
}
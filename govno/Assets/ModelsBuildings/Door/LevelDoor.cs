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
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null && (health.CurrentHealth <= 0 || health.IsDead)) return;

            isTransitioning = true;
            StartCoroutine(StableTransitionRoutine(other.gameObject));
        }
    }

    private IEnumerator StableTransitionRoutine(GameObject player)
    {
        // Стабильно обновляем чекпоинт до загрузки сцены
        if (respawnController.Instance != null)
        {
            respawnController.Instance.respawnPoint = spawnPoint;
        }

        // Даем Unity один кадр переварить смену чекпоинта
        yield return null;

        List<GameObject> objectsToPreserve = new List<GameObject>();
        objectsToPreserve.Add(player);

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.TargetTransition(targetSceneName, targetDoorID, player, objectsToPreserve);
        }
        else
        {
            Debug.LogError("[LevelDoor] TransitionManager не найден на сцене!");
        }
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
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [HideInInspector] public string targetDoorID;
    private GameObject playerObject;
    private GameObject cameraSystemRoot; // Родитель Main Camera
    private GameObject freeLookCamObject; // Явная ссылка на FreeLook

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TargetTransition(string sceneName, string doorID, GameObject player, List<GameObject> preservedObjects)
    {
        playerObject = player;
        targetDoorID = doorID;

        // Делаем игрока бессмертным
        DontDestroyOnLoad(playerObject);

        // Явный перенос ВСЕЙ системы камер
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.transform.parent != null)
            {
                cameraSystemRoot = mainCam.transform.parent.gameObject;
            }
            else
            {
                cameraSystemRoot = mainCam.gameObject;
            }
            DontDestroyOnLoad(cameraSystemRoot);
            Debug.Log($"[TransitionManager] Система Main Camera '{cameraSystemRoot.name}' сохранена.");
        }

        // ИСПРАВЛЕНО: Используем FindAnyObjectByType вместо устаревшего FindObjectOfType
        CinemachineFreeLook freeLookCam = Object.FindAnyObjectByType<CinemachineFreeLook>();
        if (freeLookCam != null)
        {
            freeLookCamObject = freeLookCam.gameObject;
            DontDestroyOnLoad(freeLookCamObject);
            Debug.Log($"[TransitionManager] Явно сохранена FreeLook камера '{freeLookCamObject.name}'.");
        }
        else
        {
            Debug.LogError("[TransitionManager] КРИТИЧЕСКАЯ ОШИБКА! На сцене не найдена CinemachineFreeLook для переноса.");
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Загружаем новую сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Ждем один кадр для инициализации объектов сцены
        yield return null;

        // Ищем дверь на новой сцене
        LevelDoor[] doors = Object.FindObjectsByType<LevelDoor>(FindObjectsSortMode.None);
        LevelDoor targetDoor = null;

        foreach (var door in doors)
        {
            if (door.doorID == targetDoorID)
            {
                targetDoor = door;
                break;
            }
        }

        if (targetDoor != null && playerObject != null)
        {
            // 1. Отключаем CharacterController, чтобы жестко выставить координаты
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. Получаем мировые координаты точки спавна новой двери
            Vector3 worldSpawnPos = targetDoor.spawnPoint.position;
            Quaternion worldSpawnRot = targetDoor.spawnPoint.rotation;

            // 3. Ставим игрока в нужную позицию и разворачиваем его наружу
            playerObject.transform.SetPositionAndRotation(worldSpawnPos, worldSpawnRot);
            Debug.Log($"[TransitionManager] Игрок телепортирован к двери {targetDoorID}.");

            // 4. Включаем физику игрока обратно
            if (cc != null) cc.enabled = true;

            // 5. МАГИЯ CINEMACHINE: Привязываем ПЕРЕЕХАВШУЮ FreeLook камеру
            CinemachineFreeLook activeFreeLookCam = null;

            // ИСПРАВЛЕНО: Ищем по всей новой сцене через современный FindAnyObjectByType
            activeFreeLookCam = Object.FindAnyObjectByType<CinemachineFreeLook>();

            if (activeFreeLookCam != null)
            {
                // Заставляем её снова следить и смотреть на игрока
                activeFreeLookCam.Follow = playerObject.transform;
                activeFreeLookCam.LookAt = playerObject.transform;
                Debug.Log($"[TransitionManager] FreeLook камера '{activeFreeLookCam.name}' (переехавшая!) привязана к игроку.");

                // Исправляем ракурс: Принудительно выставляем вертикальную ось в средний риг
                activeFreeLookCam.m_YAxis.Value = 0.5f;
                // Принудительно выставляем горизонтальную ось наружу от двери
                activeFreeLookCam.m_XAxis.Value = 0f;

                // Жестко сбрасываем позицию камеры к игроку в этом кадре
                activeFreeLookCam.ForceCameraPosition(worldSpawnPos - (worldSpawnRot * Vector3.forward * 5f) + (Vector3.up * 3f), worldSpawnRot);
            }
            else
            {
                Debug.LogWarning("[TransitionManager] На новой сцене не найдена CinemachineFreeLook для привязки!");
            }

            // Удаление второго AudioListener если он есть
            AudioListener[] allAudioListeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (allAudioListeners.Length > 1)
            {
                Debug.LogWarning($"[TransitionManager] Найдено {allAudioListeners.Length} AudioListeners. Удаляем дубликат.");
                if (Camera.main != null && Camera.main.gameObject.GetComponent<AudioListener>() != null)
                {
                    foreach (var al in allAudioListeners)
                    {
                        if (al.gameObject != Camera.main.gameObject)
                        {
                            Destroy(al);
                        }
                    }
                }
            }

            // Ждем два кадра, чтобы сцена утряслась
            yield return null;
            yield return null;

            // 6. Обновляем точку чекпоинта
            // ИСПРАВЛЕНО: Ищем менеджер респауна через современный FindAnyObjectByType
            respawnController rc = Object.FindAnyObjectByType<respawnController>();
            if (rc != null)
            {
                rc.respawnPoint = targetDoor.spawnPoint;
                Debug.Log($"[TransitionManager] Точка респауна '{rc.name}' успешно обновлена.");
            }
            else
            {
                // Супер-хак на случай тотального сбоя сцены
                Debug.LogError($"[TransitionManager] СУПЕР-КРИТИЧЕСКАЯ ОШИБКА! respawnController не найден! Применяем принудительное удержание позиции.");
                if (targetDoor.spawnPoint != null)
                {
                    targetDoor.spawnPoint.SetParent(playerObject.transform);
                }
            }
        }
        else
        {
            Debug.LogError($"[TransitionManager] Ошибка перехода! Дверь '{targetDoorID}' не найдена на сцене {sceneName}!");
        }
    }
}
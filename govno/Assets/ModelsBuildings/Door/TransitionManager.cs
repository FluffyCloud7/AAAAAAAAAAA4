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
    private GameObject cameraSystemRoot;

    private GameObject savedHeavyObject;
    private List<GameObject> savedFloatingObjects = new List<GameObject>();

    // --- СИСТЕМА ПЕРЕМЕЩЕНИЯ ПРЕДМЕТОВ НА ПОЛУ ---
    private class TrackedFloorItem
    {
        public GameObject gameObject;
        public string uniqueID;
        public string sceneName;
        public Vector3 position;
        public Quaternion rotation;
    }
    private List<TrackedFloorItem> persistentFloorItems = new List<TrackedFloorItem>();
    // --------------------------------------------

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

        Debug.Log($"<color=cyan>[Transition] Старт перехода на сцену: {sceneName}, Дверь ID: {doorID}</color>");

        PlayerGrabIso grabScript = player.GetComponent<PlayerGrabIso>();
        if (grabScript != null)
        {
            savedHeavyObject = grabScript.GetHeavyObject();
            if (savedHeavyObject != null) DontDestroyOnLoad(savedHeavyObject);

            List<GameObject> playerFloating = grabScript.GetFloatingObjectsList();
            savedFloatingObjects.Clear();
            if (playerFloating != null)
            {
                foreach (GameObject obj in playerFloating)
                {
                    if (obj != null)
                    {
                        DontDestroyOnLoad(obj);
                        savedFloatingObjects.Add(obj);
                    }
                }
            }
        }

        // Сохраняем предметы на полу перед уходом
        SaveCurrentSceneFloorItems();

        DontDestroyOnLoad(playerObject);

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraSystemRoot = mainCam.transform.parent != null ? mainCam.transform.parent.gameObject : mainCam.gameObject;
            DontDestroyOnLoad(cameraSystemRoot);
            Debug.Log($"[Transition] Корневой объект камеры {cameraSystemRoot.name} защищен через DontDestroyOnLoad.");
        }

        CinemachineFreeLook freeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCam != null)
        {
            DontDestroyOnLoad(freeLookCam.gameObject);
            Debug.Log($"[Transition] Синемашина {freeLookCam.gameObject.name} защищена через DontDestroyOnLoad.");
        }

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        Debug.Log($"<color=green>[Transition] 1. Сцена {sceneName} успешно загружена асинхронно.</color>");

        CleanupAudioListeners();

        // --- ФИКС ДУБЛИКАТА ИГРОКА ---
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"[Transition] 2. Найдено объектов с тегом Player на новой сцене: {players.Length}");
        foreach (GameObject p in players)
        {
            if (p != playerObject)
            {
                Debug.Log($"[Transition] Удаляем дубликат игрока из файла сцены: {p.name}");
                Destroy(p);
            }
        }

        LevelDoor[] doors = FindObjectsByType<LevelDoor>(FindObjectsSortMode.None);
        LevelDoor targetDoor = null;

        foreach (var door in doors)
        {
            if (door.doorID == targetDoorID)
            {
                targetDoor = door;
                break;
            }
        }

        if (targetDoor == null)
        {
            Debug.LogError($"[Transition] ОШИБКА: Целевая дверь с ID '{targetDoorID}' НЕ НАЙДЕНА на сцене {sceneName}!");
        }

        if (targetDoor != null && playerObject != null)
        {
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 worldSpawnPos = targetDoor.spawnPoint.position;
            Quaternion worldSpawnRot = targetDoor.spawnPoint.rotation;

            playerObject.transform.SetPositionAndRotation(worldSpawnPos, worldSpawnRot);
            Debug.Log($"[Transition] 3. Игрок {playerObject.name} телепортирован в точку {worldSpawnPos}");

            yield return null;
            if (cc != null) cc.enabled = true;

            if (respawnController.Instance != null)
            {
                respawnController.Instance.respawnPoint = targetDoor.spawnPoint;
            }

            // Восстановление предметов пола
            RestoreSceneFloorItems(sceneName);

            // Удаление дубликатов предметов (Твой оригинальный код)
            Debug.Log("[Transition] 4. Запуск оригинальной очистки дубликатов по именам...");
            GrabbableItem[] itemsOnScene = FindObjectsByType<GrabbableItem>(FindObjectsSortMode.None);
            foreach (var item in itemsOnScene)
            {
                if (item == null) continue;
                if (item.gameObject == savedHeavyObject || savedFloatingObjects.Contains(item.gameObject)) continue;
                if (savedHeavyObject != null && item.itemName == savedHeavyObject.GetComponent<GrabbableItem>().itemName) { Destroy(item.gameObject); continue; }
                foreach (GameObject floatObj in savedFloatingObjects)
                {
                    if (floatObj != null && item.itemName == floatObj.GetComponent<GrabbableItem>().itemName) { Destroy(item.gameObject); break; }
                }
            }

            PlayerGrabIso grabScript = playerObject.GetComponent<PlayerGrabIso>();
            if (grabScript != null)
            {
                grabScript.RestoreGrabbedItems(savedHeavyObject, savedFloatingObjects);
                Debug.Log("[Transition] 5. Удерживаемые предметы возвращены в руки игрока.");
            }

            savedHeavyObject = null;
            savedFloatingObjects.Clear();

            // --- ТВОЯ КАМЕРА ---
            Debug.Log("[Transition] 6. Начинаем поиск Синемашины на сцене...");
            CinemachineFreeLook activeFreeLookCam = FindObjectOfType<CinemachineFreeLook>();

            if (activeFreeLookCam != null)
            {
                Debug.Log($"<color=yellow>[Transition] КАМЕРА НАЙДЕНА: {activeFreeLookCam.gameObject.name}. Настраиваем следование за {playerObject.name}...</color>");
                activeFreeLookCam.Follow = playerObject.transform;
                activeFreeLookCam.LookAt = playerObject.transform;
                activeFreeLookCam.m_YAxis.Value = 0.5f;
                activeFreeLookCam.m_XAxis.Value = 0f;
                activeFreeLookCam.ForceCameraPosition(worldSpawnPos - (worldSpawnRot * Vector3.forward * 5f) + (Vector3.up * 3f), worldSpawnRot);
                Debug.Log("<color=green>[Transition] НАСТРОЙКА КАМЕРЫ УСПЕШНО ЗАВЕРШЕНА!</color>");
            }
            else
            {
                Debug.LogError("<color=red>[Transition] КРИТИЧЕСКАЯ ОШИБКА: FindObjectOfType<CinemachineFreeLook>() вернул NULL! Камера вообще не найдена на сцене!</color>");
            }
        }
    }

    private void CleanupAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++) Destroy(listeners[i]);
        }
    }

    private void SaveCurrentSceneFloorItems()
    {
        try
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            GrabbableItem[] itemsOnFloor = FindObjectsByType<GrabbableItem>(FindObjectsSortMode.None);
            int savedCount = 0;

            foreach (var item in itemsOnFloor)
            {
                if (item == null) continue;
                if (item.gameObject == savedHeavyObject || savedFloatingObjects.Contains(item.gameObject)) continue;

                string id = string.IsNullOrEmpty(item.uniqueID) ? item.gameObject.name : item.uniqueID;

                TrackedFloorItem tracked = persistentFloorItems.Find(x => x.gameObject == item.gameObject);
                if (tracked == null)
                {
                    tracked = new TrackedFloorItem();
                    tracked.gameObject = item.gameObject;
                    persistentFloorItems.Add(tracked);
                }

                tracked.uniqueID = id;
                tracked.sceneName = currentSceneName;
                tracked.position = item.transform.position;
                tracked.rotation = item.transform.rotation;

                item.transform.SetParent(null);
                DontDestroyOnLoad(item.gameObject);
                item.gameObject.SetActive(false);
                savedCount++;
            }
            Debug.Log($"[FloorItems] Сохранено предметов на полу: {savedCount}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FloorItems] Ошибка при сохранении предметов пола: {e.Message}");
        }
    }

    private void RestoreSceneFloorItems(string currentSceneName)
    {
        try
        {
            GrabbableItem[] sceneSpawnedItems = FindObjectsByType<GrabbableItem>(FindObjectsSortMode.None);
            HashSet<string> activeTrackedIDs = new HashSet<string>();

            if (savedHeavyObject != null)
            {
                var gi = savedHeavyObject.GetComponent<GrabbableItem>();
                if (gi != null) activeTrackedIDs.Add(string.IsNullOrEmpty(gi.uniqueID) ? savedHeavyObject.name : gi.uniqueID);
            }
            foreach (GameObject floatObj in savedFloatingObjects)
            {
                if (floatObj != null)
                {
                    var gi = floatObj.GetComponent<GrabbableItem>();
                    if (gi != null) activeTrackedIDs.Add(string.IsNullOrEmpty(gi.uniqueID) ? floatObj.name : gi.uniqueID);
                }
            }
            foreach (var tracked in persistentFloorItems)
            {
                if (tracked != null) activeTrackedIDs.Add(tracked.uniqueID);
            }

            int destroyedCount = 0;
            foreach (var item in sceneSpawnedItems)
            {
                if (item == null) continue;
                if (item.gameObject == savedHeavyObject || savedFloatingObjects.Contains(item.gameObject)) continue;

                string id = string.IsNullOrEmpty(item.uniqueID) ? item.gameObject.name : item.uniqueID;
                if (activeTrackedIDs.Contains(id))
                {
                    Destroy(item.gameObject);
                    destroyedCount++;
                }
            }
            Debug.Log($"[FloorItems] Уничтожено дефолтных дубликатов сцены: {destroyedCount}");

            int restoredCount = 0;
            for (int i = persistentFloorItems.Count - 1; i >= 0; i--)
            {
                var tracked = persistentFloorItems[i];
                if (tracked == null || tracked.gameObject == null)
                {
                    persistentFloorItems.RemoveAt(i);
                    continue;
                }

                if (tracked.sceneName == currentSceneName)
                {
                    tracked.gameObject.transform.SetParent(null);
                    tracked.gameObject.transform.position = tracked.position;
                    tracked.gameObject.transform.rotation = tracked.rotation;
                    tracked.gameObject.SetActive(true);

                    SceneManager.MoveGameObjectToScene(tracked.gameObject, SceneManager.GetActiveScene());
                    restoredCount++;
                }
            }
            Debug.Log($"[FloorItems] Возвращено предметов на пол текущей сцены: {restoredCount}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FloorItems] Ошибка при восстановлении пола (подавлена): {e.Message}");
        }
    }
}
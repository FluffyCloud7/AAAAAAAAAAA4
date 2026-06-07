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
    private CinemachineFreeLook persistentFreeLookCam; // Основная Синемашина

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

        // Кэшируем оригинальную Синемашину перед уничтожением сцены
        persistentFreeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (persistentFreeLookCam != null)
        {
            DontDestroyOnLoad(persistentFreeLookCam.gameObject);
            Debug.Log($"[Transition] Синемашина {persistentFreeLookCam.gameObject.name} защищена через DontDestroyOnLoad.");
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

        // Безопасная очистка дубликатов аудиослушателей
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

        // --- ФИКС ДУБЛИКАТОВ КАМЕРЫ И СИНЕМАШИНЫ (С ИСКЛЮЧЕНИЕМ ДЛЯ VCam_Mouse) ---
        CinemachineFreeLook[] freeLookCams = FindObjectsByType<CinemachineFreeLook>(FindObjectsSortMode.None);
        foreach (var cam in freeLookCams)
        {
            if (persistentFreeLookCam != null && cam != persistentFreeLookCam)
            {
                Debug.Log($"[Transition] Удаляем дубликат Синемашины из файла новой сцены: {cam.gameObject.name}");
                Destroy(cam.gameObject);
            }
        }

        // Удаляем только дефолтные MainCamera новой сцены, НЕ трогая систему мыши и оффсетов
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (cameraSystemRoot != null && !cam.transform.IsChildOf(cameraSystemRoot.transform) && cam.gameObject != cameraSystemRoot)
            {
                if (cam.CompareTag("MainCamera") && cam.gameObject.name != "VCam_Mouse")
                {
                    GameObject rootToDestroy = cam.transform.parent != null ? cam.transform.parent.gameObject : cam.gameObject;
                    Debug.Log($"[Transition] Удаляем дубликат основной камеры сцены: {rootToDestroy.name}");
                    Destroy(rootToDestroy);
                }
            }
        }
        // --------------------------------------------

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

            // Удаление дубликатов предметов
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

            // --- НАСТРОЙКА ГЛАВНОЙ СИНЕМАШИНЫ FREELOOK ---
            if (persistentFreeLookCam == null)
            {
                persistentFreeLookCam = FindObjectOfType<CinemachineFreeLook>();
            }

            if (persistentFreeLookCam != null)
            {
                Debug.Log($"<color=yellow>[Transition] Настраиваем сохраненную Синемашину {persistentFreeLookCam.gameObject.name}...</color>");
                persistentFreeLookCam.Follow = playerObject.transform;
                persistentFreeLookCam.LookAt = playerObject.transform;
                persistentFreeLookCam.m_YAxis.Value = 0.5f;
                persistentFreeLookCam.m_XAxis.Value = 0f;
                persistentFreeLookCam.ForceCameraPosition(worldSpawnPos - (worldSpawnRot * Vector3.forward * 5f) + (Vector3.up * 3f), worldSpawnRot);
            }
            else
            {
                Debug.LogError("<color=red>[Transition] КРИТИЧЕСКАЯ ОШИБКА: Синемашина Freelook потеряна при переходе!</color>");
            }

            // --- ПЕРЕПРИВЯЗКА ТВОЕЙ КАМЕРЫ МЫШИ (VCam_Mouse) ---
            CinemachineVirtualCamera[] virtualCameras = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
            bool mouseCamFound = false;
            foreach (var vcam in virtualCameras)
            {
                if (vcam.gameObject.name == "VCam_Mouse")
                {
                    vcam.Follow = playerObject.transform;
                    mouseCamFound = true;
                    Debug.Log("<color=green>[Transition] Камера VCam_Mouse успешно найдена на новой сцене и привязана к игроку!</color>");
                    break;
                }
            }
            if (!mouseCamFound)
            {
                Debug.LogWarning("[Transition] Предупреждение: Камера 'VCam_Mouse' не обнаружена в файле этой сцены.");
            }
        }
    }

    private void CleanupAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in listeners)
        {
            if (listener == null) continue;

            if (cameraSystemRoot != null)
            {
                if (listener.transform.IsChildOf(cameraSystemRoot.transform) || listener.gameObject == cameraSystemRoot)
                {
                    continue;
                }
            }

            Debug.Log($"[Transition] Удаляем дубликат AudioListener на объекте: {listener.gameObject.name}");
            Destroy(listener);
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

                string id = item.gameObject.name;

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
                if (gi != null) activeTrackedIDs.Add(savedHeavyObject.name);
            }
            foreach (GameObject floatObj in savedFloatingObjects)
            {
                if (floatObj != null)
                {
                    var gi = floatObj.GetComponent<GrabbableItem>();
                    if (gi != null) activeTrackedIDs.Add(floatObj.name);
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

                string id = item.gameObject.name;
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
                    persistentFreeLookCam = null;
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
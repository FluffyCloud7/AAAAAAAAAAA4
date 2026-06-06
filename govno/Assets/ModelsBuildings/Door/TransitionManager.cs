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

    // Временное хранилище для переносимых предметов (куба и хоровода)
    private GameObject savedHeavyObject;
    private List<GameObject> savedFloatingObjects = new List<GameObject>();

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

    // Тот самый оригинальный рабочий метод перехода
    public void TargetTransition(string sceneName, string doorID, GameObject player, List<GameObject> preservedObjects)
    {
        playerObject = player;
        targetDoorID = doorID;

        // 1. ЗАБИРАЕМ ССЫЛКИ НА ПРЕДМЕТЫ ИЗ РУК И ХОРОВОДА ПЕРЕД ПЕРЕХОДОМ
        PlayerGrabIso grabScript = player.GetComponent<PlayerGrabIso>();
        if (grabScript != null)
        {
            savedHeavyObject = grabScript.GetHeavyObject();
            if (savedHeavyObject != null)
            {
                DontDestroyOnLoad(savedHeavyObject);
            }

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

        // Защищаем игрока от уничтожения
        DontDestroyOnLoad(playerObject);

        // Перенос камерной системы
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraSystemRoot = mainCam.transform.parent != null ? mainCam.transform.parent.gameObject : mainCam.gameObject;
            DontDestroyOnLoad(cameraSystemRoot);
        }

        CinemachineFreeLook freeLookCam = FindObjectOfType<CinemachineFreeLook>();
        if (freeLookCam != null)
        {
            DontDestroyOnLoad(freeLookCam.gameObject);
        }

        // ЗАПУСКАЕМ ТУ САМУЮ РАБОЧУЮ КОРУТИНУ
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    // Рабочая корутина с безопасными таймингами для Unity 6
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Асинхронная загрузка, которая даёт сцене время подготовиться
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // КРИТИЧЕСКИ ВАЖНО: Ждем кадры, чтобы сцена, старые синглтоны и физика полностью проснулись
        yield return new WaitForEndOfFrame();
        yield return null;

        // Находим целевую дверь на новой сцене
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

        if (targetDoor != null && playerObject != null)
        {
            // 2. ОТКЛЮЧАЕМ ХИТРОЖОПЫЙ ХИТБОКС И ФИЗИКУ (Убивает ложные срабатывания чекпоинтов)
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 worldSpawnPos = targetDoor.spawnPoint.position;
            Quaternion worldSpawnRot = targetDoor.spawnPoint.rotation;

            // Перемещаем игрока ровно в точку двери
            playerObject.transform.SetPositionAndRotation(worldSpawnPos, worldSpawnRot);

            // Даем один кадр на фиксацию координат в пространстве
            yield return null;

            // Включаем физику обратно только тогда, когда игрок уже ТОЧНО стоит у двери
            if (cc != null) cc.enabled = true;

            // 3. НАМЕРТВО ПЕРЕЗАПИСЫВАЕМ ЧЕКПОИНТ (Перекрываем старые данные респауна)
            if (respawnController.Instance != null)
            {
                respawnController.Instance.respawnPoint = targetDoor.spawnPoint;
            }

            // 4. УДАЛЕНИЕ СТАТИЧНЫХ ДЮПОВ ИЗ СЦЕНЫ
            GrabbableItem[] itemsOnScene = FindObjectsByType<GrabbableItem>(FindObjectsSortMode.None);
            foreach (var item in itemsOnScene)
            {
                // Пропускаем объекты, приехавшие с игроком
                if (item.gameObject == savedHeavyObject || savedFloatingObjects.Contains(item.gameObject)) continue;

                // Если имя совпало с тяжелым кубом в руках — сносим дубликат со сцены
                if (savedHeavyObject != null)
                {
                    GrabbableItem heavyComp = savedHeavyObject.GetComponent<GrabbableItem>();
                    if (heavyComp != null && item.itemName == heavyComp.itemName)
                    {
                        Destroy(item.gameObject);
                        continue;
                    }
                }

                // Если имя совпало с предметом из хоровода — сносим дубликат со сцены
                foreach (GameObject floatObj in savedFloatingObjects)
                {
                    if (floatObj != null)
                    {
                        GrabbableItem floatComp = floatObj.GetComponent<GrabbableItem>();
                        if (floatComp != null && item.itemName == floatComp.itemName)
                        {
                            Destroy(item.gameObject);
                            break;
                        }
                    }
                }
            }

            // 5. ПРИНУДИТЕЛЬНО ВОЗВРАЩАЕМ ПРЕДМЕТЫ В ЛОГИКУ ИГРОКА
            PlayerGrabIso grabScript = playerObject.GetComponent<PlayerGrabIso>();
            if (grabScript != null)
            {
                grabScript.RestoreGrabbedItems(savedHeavyObject, savedFloatingObjects);
            }

            // Очищаем кэш ссылок менеджера
            savedHeavyObject = null;
            savedFloatingObjects.Clear();

            // 6. СТАБИЛИЗАЦИЯ КАМЕРЫ CINEMACHINE ПОСЛЕ ТЕЛЕПОРТА
            CinemachineFreeLook activeFreeLookCam = FindObjectOfType<CinemachineFreeLook>();
            if (activeFreeLookCam != null)
            {
                activeFreeLookCam.Follow = playerObject.transform;
                activeFreeLookCam.LookAt = playerObject.transform;
                activeFreeLookCam.m_YAxis.Value = 0.5f;
                activeFreeLookCam.m_XAxis.Value = 0f;
                activeFreeLookCam.ForceCameraPosition(worldSpawnPos - (worldSpawnRot * Vector3.forward * 5f) + (Vector3.up * 3f), worldSpawnRot);
            }
        }
        else
        {
            Debug.LogError($"[TransitionManager] Не удалось найти целевую дверь '{targetDoorID}' на новой сцене!");
        }
    }
}
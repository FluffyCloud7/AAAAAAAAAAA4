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

        DontDestroyOnLoad(playerObject);

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
        CleanupAudioListeners();

        // --- ФИКС ДУБЛИКАТА ИГРОКА ---
        // Находим всех персонажей на сцене и удаляем тех, кто не является нашим перенесенным игроком
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p != playerObject)
            {
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

        if (targetDoor != null && playerObject != null)
        {
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 worldSpawnPos = targetDoor.spawnPoint.position;
            Quaternion worldSpawnRot = targetDoor.spawnPoint.rotation;

            playerObject.transform.SetPositionAndRotation(worldSpawnPos, worldSpawnRot);

            yield return null;
            if (cc != null) cc.enabled = true;

            if (respawnController.Instance != null)
            {
                respawnController.Instance.respawnPoint = targetDoor.spawnPoint;
            }

            // Удаление дубликатов предметов
            GrabbableItem[] itemsOnScene = FindObjectsByType<GrabbableItem>(FindObjectsSortMode.None);
            foreach (var item in itemsOnScene)
            {
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
            }

            savedHeavyObject = null;
            savedFloatingObjects.Clear();

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
    }

    private void CleanupAudioListeners()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++) Destroy(listeners[i]);
        }
    }
}
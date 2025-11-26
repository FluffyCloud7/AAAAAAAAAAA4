using UnityEngine;

public class BoxSpawner : MonoBehaviour
{
    public GameObject boxPrefab;   // префаб куба
    public Transform spawnPoint;   // место спавна
    private GameObject spawnedBox; // ссылка на текущий куб

    private bool playerInside = false;

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            SpawnOrRespawnBox();
        }
    }

    void SpawnOrRespawnBox()
    {
        if (spawnedBox != null)
        {
            // если куб уже есть → телепортируем на SpawnPoint
            spawnedBox.transform.position = spawnPoint.position;
            spawnedBox.transform.rotation = Quaternion.identity;
            // если нужно, сбрасываем скорость Rigidbody
            Rigidbody rb = spawnedBox.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            // создаем новый куб
            spawnedBox = Instantiate(boxPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}

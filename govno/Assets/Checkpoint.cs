using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SaveSystem.SaveCheckpoint(checkpointID, transform.position);
            Debug.Log("Сохранена точка #" + checkpointID);
        }
    }
}

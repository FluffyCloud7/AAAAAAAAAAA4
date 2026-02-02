using UnityEngine;

public class CheckPoint2 : MonoBehaviour
{
    public CheckPoint2 trigger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") )
        {
            respawnController.Instance.respawnPoint = transform;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth();
            }

            trigger.enabled = false;
        }
    }
}

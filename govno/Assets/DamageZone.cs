using UnityEngine;
using System.Collections;

public class DamageZone : MonoBehaviour
{
    public int damageAmount = 1;
    public float damageInterval = 100f;

    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += () => StopCoroutine(damageCoroutine);
                if (playerHealth.CurrentHealth >= 0)
                damageCoroutine = StartCoroutine(DamageLoop(playerHealth));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¿¿¿¿¿¿¿¿¿");

            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);
        }
    }

    private IEnumerator DamageLoop(PlayerHealth player)
    {
        while (player != null && !player.IsDead)
        {
            player.TakeDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
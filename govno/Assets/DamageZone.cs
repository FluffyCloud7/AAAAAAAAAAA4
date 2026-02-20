using UnityEngine;
using System.Collections;

public class DamageZone : MonoBehaviour
{
    public int damageAmount = 1;
    public float damageInterval = 1f;

    private Coroutine damageCoroutine;
    private PlayerHealth currentPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        currentPlayer = other.GetComponent<PlayerHealth>();
        if (currentPlayer == null) return;

        damageCoroutine = StartCoroutine(DamageLoop());
        currentPlayer.OnRespawn += StopDamage;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StopDamage();
    }

    private IEnumerator DamageLoop()
    {
        while (currentPlayer != null && !currentPlayer.IsDead)
        {
            currentPlayer.TakeDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void StopDamage()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (currentPlayer != null)
        {
            currentPlayer.OnRespawn -= StopDamage;
            currentPlayer = null;
        }
    }
}
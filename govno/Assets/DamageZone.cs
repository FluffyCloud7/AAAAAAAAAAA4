using UnityEngine;
using System.Collections;

public class DamageZone : MonoBehaviour
{
    public int damageAmount = 1;
    public float damageInterval = 1f;

    Coroutine damageCoroutine;
    PlayerHealth currentPlayer;
    InkPuddleMask puddle;

    void Start()
    {
        puddle = GetComponentInParent<InkPuddleMask>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        currentPlayer = other.GetComponent<PlayerHealth>();
        if (currentPlayer == null) return;

        damageCoroutine = StartCoroutine(DamageLoop());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StopDamage();
    }

    IEnumerator DamageLoop()
    {
        while (currentPlayer != null && !currentPlayer.IsDead)
        {
            if (PlayerStandingOnInk())
                currentPlayer.TakeDamage(damageAmount);

            yield return new WaitForSeconds(damageInterval);
        }
    }

    bool PlayerStandingOnInk()
    {
        Vector3 origin = currentPlayer.transform.position;

        float checkRadius = 0.35f;

        Vector3[] points =
        {
        origin,
        origin + Vector3.forward * checkRadius,
        origin - Vector3.forward * checkRadius,
        origin + Vector3.right * checkRadius,
        origin - Vector3.right * checkRadius
    };

        foreach (var p in points)
        {
            Ray ray = new Ray(p + Vector3.up * 0.2f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 2f, ~0, QueryTriggerInteraction.Ignore))
            {
                InkPuddleMask mask = hit.collider.GetComponent<InkPuddleMask>();

                if (mask != null && mask.HasInk(hit.textureCoord))
                    return true;
            }
        }

        return false;
    }

    void StopDamage()
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = null;
        currentPlayer = null;
    }
}
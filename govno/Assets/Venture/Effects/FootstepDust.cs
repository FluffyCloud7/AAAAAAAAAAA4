using UnityEngine;

public class FootstepDust : MonoBehaviour
{
    [Header("Префабы пыли")]
    [SerializeField] private ParticleSystem walkDustPrefab;
    [SerializeField] private ParticleSystem runDustPrefab;

    [Header("Точки для ХОДЬБЫ")]
    [SerializeField] private Transform leftFootWalkPoint;
    [SerializeField] private Transform rightFootWalkPoint;

    [Header("Точки для БЕГА")]
    [SerializeField] private Transform leftFootRunPoint;
    [SerializeField] private Transform rightFootRunPoint;

    // --- МЕТОДЫ ДЛЯ ХОДЬБЫ ---
    public void TriggerLeftFootWalkDust()
    {
        SpawnDust(leftFootWalkPoint.position, walkDustPrefab);
    }

    public void TriggerRightFootWalkDust()
    {
        SpawnDust(rightFootWalkPoint.position, walkDustPrefab);
    }

    // --- МЕТОДЫ ДЛЯ БЕГА ---
    public void TriggerLeftFootRunDust()
    {
        SpawnDust(leftFootRunPoint.position, runDustPrefab);
    }

    public void TriggerRightFootRunDust()
    {
        SpawnDust(rightFootRunPoint.position, runDustPrefab);
    }

    // Универсальный спавн
    private void SpawnDust(Vector3 position, ParticleSystem prefab)
    {
        if (prefab != null)
        {
            ParticleSystem dustInstance = Instantiate(prefab, position, transform.rotation);
            dustInstance.Play();
            Destroy(dustInstance.gameObject, dustInstance.main.duration + dustInstance.main.startLifetime.constantMax);
        }
    }

    [Header("Префаб для ПРИЗЕМЛЕНИЯ")]
    [SerializeField] private ParticleSystem landDustPrefab;

    // Этот метод мы будем вызывать при приземлении
    public void TriggerLandingDust()
    {
        if (landDustPrefab == null) return;

        // Берем позицию центра персонажа на земле
        Vector3 spawnPosition = transform.position;

        // Спавним пыль. Так как форма круга уже развернута в префабе (X = 90),
        // мы можем использовать Quaternion.identity, чтобы она легла ровно на землю.
        ParticleSystem dustInstance = Instantiate(landDustPrefab, spawnPosition, Quaternion.identity);
        dustInstance.Play();

        Destroy(dustInstance.gameObject, dustInstance.main.duration + dustInstance.main.startLifetime.constantMax);
    }
}
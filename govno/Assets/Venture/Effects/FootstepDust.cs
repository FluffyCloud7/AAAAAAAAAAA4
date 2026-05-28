using UnityEngine;

public class FootstepDust : MonoBehaviour
{
    [Header("Настройки пыли")]
    [SerializeField] private ParticleSystem dustPrefab;
    [SerializeField] private Transform leftFootTransform;
    [SerializeField] private Transform rightFootTransform;

    // Этот метод будет вызываться из анимации для левой ноги
    public void TriggerLeftFootDust()
    {
        SpawnDust(leftFootTransform.position);
    }

    // Этот метод для правой ноги
    public void TriggerRightFootDust()
    {
        SpawnDust(rightFootTransform.position);
    }

    private void SpawnDust(Vector3 position)
    {
        if (dustPrefab != null)
        {
            // Создаем партикл в точке ноги
            ParticleSystem dustInstance = Instantiate(dustPrefab, position, Quaternion.identity);

            // Запускаем воспроизведение
            dustInstance.Play();

            // Уничтожаем объект после того, как он отыграет, чтобы не засорять память
            Destroy(dustInstance.gameObject, dustInstance.main.duration + dustInstance.main.startLifetime.constantMax);
        }
    }
}
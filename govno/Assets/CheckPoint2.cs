using UnityEngine;

public class CheckPoint2 : MonoBehaviour
{
    public CheckPoint2 trigger;

    [Header("—сылка на спираль")]
    [Tooltip("ѕеретащи сюда объект SpiralPoint, на котором висит скрипт спирали")]
    public CorrectSpiralFloorText floorSpiral;

    [Header("—сылка на перо")]
    [Tooltip("ѕеретащи сюда модельку пера, на которой висит скрипт FeatherFloating")]
    public FeatherFloating feather;

    [Header("Ёффекты при активации")]
    [Tooltip("ѕеретащи сюда систему частиц ToTheFeather")]
    public ParticleSystem toTheFeatherParticles; // ƒќЅј¬Ћ≈Ќќ: —сылка на наши трейлы

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ќаш базовый рабочий функционал (не трогаем!)
            respawnController.Instance.respawnPoint = transform;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth();
            }

            // ј “»¬ј÷»я »«ћ≈Ќ≈Ќ»… ¬ —ѕ»–јЋ»
            if (floorSpiral != null)
            {
                floorSpiral.ActivateCheckpoint();
            }

            // ј “»¬ј÷»я —∆ј“»я ѕ≈–ј
            if (feather != null)
            {
                feather.CollectFeather();
            }

            // «јѕ”—  —¬≈“яў»’—я Ћ”„≈…
            if (toTheFeatherParticles != null)
            {
                toTheFeatherParticles.Play(); // ƒќЅј¬Ћ≈Ќќ: ¬ключаем трейлы один раз
            }

            trigger.enabled = false;
        }
    }

    // ќтрисовка границ коллайдера в окне Scene дл€ удобного центрировани€ пера
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.15f);
            Gizmos.DrawCube(transform.position + box.center, box.size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + box.center, box.size);
        }

        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.15f);
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}
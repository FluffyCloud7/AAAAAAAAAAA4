using UnityEngine;

public enum ItemSize { HeavyInHands, LightFloating }

public class GrabbableItem : MonoBehaviour
{
    public ItemSize itemSize = ItemSize.HeavyInHands;
    public string itemName = "Item";
    public string uniqueID;

    [Header("Настройки анимации")]
    public float amplitude = 0.2f;
    public float frequency = 2.0f;
    public float rotationSpeed = 45f;

    [Header("Эффекты частиц")]
    public ParticleSystem particleEffect;

    private Vector3 startPos;
    private Quaternion startRot; // Запоминаем исходный поворот
    private Rigidbody rb;        // Ссылка на физику
    private bool isPickedUp = false;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation; // Сохраняем начальное вращение
        rb = GetComponent<Rigidbody>(); // Пробуем получить Rigidbody

        UpdateEffectState();
    }

    void Update()
    {
        if (itemSize == ItemSize.HeavyInHands) return;
        if (isPickedUp) return;

        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    public void SetPickedUp(bool state)
    {
        isPickedUp = state;
        if (state && itemSize == ItemSize.LightFloating)
            transform.rotation = Quaternion.identity;

        UpdateEffectState();
    }

    private void UpdateEffectState()
    {
        if (particleEffect == null) return;

        bool shouldBeActive = (itemSize == ItemSize.LightFloating && !isPickedUp);

        if (shouldBeActive && !particleEffect.isPlaying)
        {
            particleEffect.Play();
        }
        else if (!shouldBeActive && particleEffect.isPlaying)
        {
            particleEffect.Stop();
        }
    }

    // --- НОВЫЙ МЕТОД ДЛЯ РЕСПАВНА ---
    public void Respawn()
    {
        // Перемещаем в начальную точку
        transform.position = startPos;
        transform.rotation = startRot;

        // Если у объекта есть физика, сбрасываем её импульс
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // В Unity 6 вместо velocity используется linearVelocity
            rb.angularVelocity = Vector3.zero;
        }

        // На всякий случай сообщаем, что объект больше не в руках (если его уронили в бездну вместе с игроком)
        SetPickedUp(false);
    }
}
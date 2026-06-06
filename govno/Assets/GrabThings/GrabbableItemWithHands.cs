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
    public ParticleSystem particleEffect; // Перетащите сюда ваш эффект (Niagara/Unity Particles)

    private Vector3 startPos;
    private bool isPickedUp = false;

    void Start()
    {
        startPos = transform.position;
        // При старте проверяем, нужно ли включить эффект
        UpdateEffectState();
    }

    void Update()
    {
        // Тяжелые объекты игнорируем
        if (itemSize == ItemSize.HeavyInHands) return;

        // Если подобран, анимация не нужна
        if (isPickedUp) return;

        // Покачивание
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

        // Включаем эффект, только если предмет легкий И он НЕ подобран
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
}
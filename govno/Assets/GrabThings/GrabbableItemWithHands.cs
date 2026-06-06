using UnityEngine;

public enum ItemSize { HeavyInHands, LightFloating }

public class GrabbableItem : MonoBehaviour
{
    public ItemSize itemSize = ItemSize.HeavyInHands;
    public string itemName = "Item";
    public string uniqueID;

    [Header("Настройки анимации (только для LightFloating)")]
    public float amplitude = 0.2f;
    public float frequency = 2.0f;
    public float rotationSpeed = 45f;

    private Vector3 startPos;
    private bool isPickedUp = false;

    void Start() => startPos = transform.position;

    void Update()
    {
        // Анимация работает только если предмет НЕ подобран И это легкий предмет
        if (isPickedUp || itemSize == ItemSize.HeavyInHands) return;

        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    public void SetPickedUp(bool state)
    {
        isPickedUp = state;
        // Сбрасываем поворот только если это легкий предмет, чтобы он не выглядел странно после броска
        if (state && itemSize == ItemSize.LightFloating)
            transform.rotation = Quaternion.identity;
    }
}
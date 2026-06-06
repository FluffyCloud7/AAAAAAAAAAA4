using UnityEngine;

public enum ItemSize
{
    HeavyInHands,  // “€желый (коробка/куб) Ч занимаетс€ holdParent, только один
    LightFloating  // Ћегкий (ключ/квест-айтем) Ч летит над головой, можно несколько
}

public class GrabbableItem : MonoBehaviour
{
    public ItemSize itemSize = ItemSize.HeavyInHands;

    // —юда можно дописать уникальный ID предмета или им€ дл€ логики квестов
    public string itemName = "Item";

    [Header("”никальный ID предмета дл€ сохранени€ между сценами")]
    public string uniqueID;
}
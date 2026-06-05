using UnityEngine;

public class WaterZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // »щем скрипт губки на объекте, который зашел в воду
        SpongeCleaner sponge = other.GetComponent<SpongeCleaner>();

        // ≈сли это губка Ч моем еЄ
        if (sponge != null)
        {
            sponge.WashSponge();
            Debug.Log("[WaterZone]: √убка успешно помыта в воде!");
        }
    }
}
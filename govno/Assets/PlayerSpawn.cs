using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    private void Start()
    {
        if (SaveSystem.HasCheckpoint())
        {
            transform.position = SaveSystem.LoadPosition();
            Debug.Log("Позиция игрока загружена: " + transform.position);
        }
        else
        {
            Debug.Log("Сохранений нет — старт с начальной позиции");
        }
    }
}

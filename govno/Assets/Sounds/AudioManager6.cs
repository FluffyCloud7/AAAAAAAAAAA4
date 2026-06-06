using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // «ащищаем музыку от удалени€ при переходах
        }
        else
        {
            // ≈сли мы вернулись на сцену и ёнити попыталс€ создать дубликат плеера Ч
            // мы его сразу же уничтожаем. —тара€ музыка продолжает играть без прерываний.
            Destroy(gameObject);
        }
    }
}
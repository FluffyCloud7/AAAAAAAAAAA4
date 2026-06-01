using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;

    [Header("Список фоновой музыки для рандома")]
    public List<AudioClip> musicPlaylist;

    [Header("Настройки переходов (в секундах)")]
    [SerializeField] private float fadeDuration = 2.0f; // Время затухания и нарастания

    [SerializeField] private Slider musicSlider;

    private List<AudioClip> playQueue = new List<AudioClip>();
    private AudioClip lastPlayedClip;
    private float targetVolume = 1f; // Громкость, которую выставил игрок на слайдере
    private bool isTransitioning = false; // Флаг, чтобы Update не мешал затуханию

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();

            if (audioSource != null)
            {
                audioSource.loop = false;
            }

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Сначала настраиваем ползунок, чтобы узнать целевую громкость игрока
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(delegate { SetVolume(musicSlider.value); });
            targetVolume = musicSlider.value;
        }

        if (musicPlaylist != null && musicPlaylist.Count > 0)
        {
            BuildPlayQueue();
            // Запускаем первый трек сразу с плавным нарастанием
            PlayNextTrack();
        }
    }

    private void Update()
    {
        // Если идет плавный переход — Update ничего не делает и ждет
        if (isTransitioning) return;

        // Если музыка подошла к концу (осталось меньше времени, чем длится затухание)
        if (audioSource != null && audioSource.isPlaying)
        {
            float timeRemaining = audioSource.clip.length - audioSource.time;

            // Если до конца песни осталось 2 секунды (fadeDuration) — запускаем плавную смену трека!
            if (timeRemaining <= fadeDuration)
            {
                StartCoroutine(TransitionToNextTrack());
            }
        }
        // На случай, если музыка вообще почему-то остановилась сама
        else if (audioSource != null && !audioSource.isPlaying && musicPlaylist != null && musicPlaylist.Count > 0)
        {
            StartCoroutine(TransitionToNextTrack());
        }
    }

    private void BuildPlayQueue()
    {
        playQueue = new List<AudioClip>(musicPlaylist);

        if (playQueue.Count > 1)
        {
            for (int i = playQueue.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                AudioClip temp = playQueue[i];
                playQueue[i] = playQueue[rnd];
                playQueue[rnd] = temp;
            }

            if (playQueue[0] == lastPlayedClip && playQueue.Count > 1)
            {
                playQueue.Add(playQueue[0]);
                playQueue.RemoveAt(0);
            }
        }
    }

    // Корутина плавного перехода: глушит старый трек, меняет его и разгоняет новый
    private IEnumerator TransitionToNextTrack()
    {
        isTransitioning = true;

        // 1. ПЛАВНОЕ ЗАТУХАНИЕ (Fade Out)
        float startVolume = audioSource.volume;
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Постепенно снижаем громкость от текущей до нуля
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        audioSource.Stop();

        // 2. СМЕНА ТРЕКА
        PlayNextTrack();

        // 3. ПЛАВНОЕ НАРАСТАНИЕ (Fade In)
        audioSource.volume = 0;
        currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Постепенно поднимаем громкость от нуля до targetVolume (которую выставил игрок)
            audioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

        // Жестко фиксируем финальную громкость на всякий случай
        audioSource.volume = targetVolume;

        isTransitioning = false;
    }

    private void PlayNextTrack()
    {
        if (playQueue.Count == 0)
        {
            BuildPlayQueue();
        }

        if (playQueue.Count > 0)
        {
            AudioClip nextClip = playQueue[0];
            playQueue.RemoveAt(0);

            lastPlayedClip = nextClip;
            audioSource.clip = nextClip;
            audioSource.Play();
        }
    }

    public void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null)
    {
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            if (resetSong)
            {
                audioSource.Stop();
            }
            audioSource.Play();
            audioSource.volume = targetVolume;
        }
    }

    public static void SetVolume(float volume)
    {
        if (Instance != null)
        {
            // Запоминаем новое значение слайдера как максимальную цель
            Instance.targetVolume = volume;

            // Если прямо сейчас музыка НЕ находится в процессе затухания/нарастания,
            // то мгновенно применяем громкость. Если переход идет — корутина сама настроит всё.
            if (!Instance.isTransitioning && Instance.audioSource != null)
            {
                Instance.audioSource.volume = volume;
            }
        }
    }
}
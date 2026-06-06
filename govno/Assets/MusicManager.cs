using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; } // Сделали публичным геттер для удобства
    private AudioSource audioSource;

    [Header("Список фоновой музыки для рандома")]
    public List<AudioClip> musicPlaylist;

    [Header("Настройки переходов (в секундах)")]
    [SerializeField] private float fadeDuration = 2.0f;

    private Slider musicSlider;
    private List<AudioClip> playQueue = new List<AudioClip>();
    private AudioClip lastPlayedClip;
    private float targetVolume = 1f;
    private bool isTransitioning = false;

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
        if (musicPlaylist != null && musicPlaylist.Count > 0)
        {
            BuildPlayQueue();
            PlayNextTrack();
        }
    }

    // Метод жесткой привязки конкретного ползунка из меню паузы
    public void BindSlider(Slider slider)
    {
        musicSlider = slider;
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = targetVolume; // Возвращаем ползунок на сохраненное место
            musicSlider.onValueChanged.AddListener(delegate { SetVolume(musicSlider.value); });
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        if (audioSource != null && audioSource.isPlaying)
        {
            float timeRemaining = audioSource.clip.length - audioSource.time;

            if (timeRemaining <= fadeDuration)
            {
                StartCoroutine(TransitionToNextTrack());
            }
        }
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

    private IEnumerator TransitionToNextTrack()
    {
        isTransitioning = true;

        float startVolume = audioSource.volume;
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        PlayNextTrack();

        audioSource.volume = 0;
        currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

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
            Instance.targetVolume = volume;

            if (!Instance.isTransitioning && Instance.audioSource != null)
            {
                Instance.audioSource.volume = volume;
            }
        }
    }
}
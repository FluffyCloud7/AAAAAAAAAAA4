using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;
    public AudioClip backGroundMusic;
    [SerializeField] private Slider musicSlider;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(backGroundMusic != null)
        {
            PlayBackgroundMusic(false, backGroundMusic);
        }
        musicSlider.onValueChanged.AddListener(delegate { SetVolume(musicSlider.value); });
    }
 public void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null)
    {
        if(audioClip != null)
        {
            audioSource.clip = audioClip;
        }
        if (audioClip != null)
        {
            if(resetSong)
            {
                audioSource.Stop();
            }
            audioSource.Play();
        }
    }

    public static void SetVolume(float volume)
    {
        Instance.audioSource.volume = volume;
    }
}

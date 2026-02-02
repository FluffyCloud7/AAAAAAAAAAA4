using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{

    private static SoundEffectManager Instance;
    private static AudioSource audioSource;
    private static AudioSource voiceaudioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Play(string soundName)
    {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if(audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    //public static void PlayVoice(AudioClip audioClip, float pitch = 1f)
    //{
        //voiceaudioSource.pitch = pitch;
        //voiceaudioSource.PlayOneShot(audioClip);
   // }

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
        //voiceaudioSource.volume = volume;
    }

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }
}

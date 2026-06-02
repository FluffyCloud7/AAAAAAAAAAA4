using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager Instance;
    private static AudioSource audioSource;
    private static AudioSource voiceaudioSource; // Источник для озвучки букв/голоса/шагов/прыжков
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;

    private static string currentVoiceGroupName; // Название группы звуков из библиотеки
    private static bool isVoiceLooping = false;  // Флаг, идет ли сейчас печать текста
    private static float baseVoicePitch = 1f;    // Храним базовый питч из настроек диалога
    private static float currentVolumeMultiplier = 1f; // Храним индивидуальный множитель громкости

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Получаем все AudioSource на этом объекте
            AudioSource[] sources = GetComponents<AudioSource>();

            if (sources.Length > 0) audioSource = sources[0]; // Первый — для обычных SFX (урон и т.д.)
            if (sources.Length > 1) voiceaudioSource = sources[1]; // Второй — для голоса, шагов и прыжков

            // Если второго источника нет, создадим его автоматически
            if (voiceaudioSource == null && audioSource != null)
            {
                voiceaudioSource = gameObject.AddComponent<AudioSource>();
            }

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
        if (soundEffectLibrary == null || audioSource == null) return;

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
            SetVolume(sfxSlider.value);
        }
    }

    // ИСПРАВЛЕНО: Добавлен параметр volumeMultiplier для точечной настройки громкости (например, приземления)
    public static void PlayVoice(string groupName, float pitch = 1f, bool loop = false, float volumeMultiplier = 1f)
    {
        if (voiceaudioSource == null || soundEffectLibrary == null) return;

        currentVoiceGroupName = groupName;
        isVoiceLooping = loop;
        baseVoicePitch = pitch; // Запоминаем базовый питч
        currentVolumeMultiplier = volumeMultiplier; // Запоминаем текущий множитель громкости

        // Применяем случайную тональность вокруг базовой (от -15% до +15%)
        voiceaudioSource.pitch = baseVoicePitch * Random.Range(0.85f, 1.15f);

        // Настраиваем громкость источника с учетом ползунка и множителя
        UpdateVoiceSourceVolume();

        // Если это короткий звук (буквы, прыжки, шаги, приземление)
        if (!loop)
        {
            AudioClip clip = soundEffectLibrary.GetRandomClip(groupName);
            if (clip != null) voiceaudioSource.PlayOneShot(clip, currentVolumeMultiplier);
        }
        // Если это длинный карандаш и он еще не начал играть
        else if (!voiceaudioSource.isPlaying)
        {
            PlayNextRandomVoiceClip();
        }
    }

    // Берет следующий случайный чирк из библиотеки и тоже рандомизирует ему питч
    private static void PlayNextRandomVoiceClip()
    {
        if (!isVoiceLooping || voiceaudioSource == null || soundEffectLibrary == null) return;

        AudioClip nextClip = soundEffectLibrary.GetRandomClip(currentVoiceGroupName);
        if (nextClip != null)
        {
            voiceaudioSource.clip = nextClip;
            // Снова крутим питч для следующего звука в очереди, чтобы они отличались
            voiceaudioSource.pitch = baseVoicePitch * Random.Range(0.85f, 1.15f);

            UpdateVoiceSourceVolume();
            voiceaudioSource.Play();
        }
    }

    private void Update()
    {
        // Если текст еще пишется, а прошлый случайный звук закончился — запускаем следующий
        if (isVoiceLooping && voiceaudioSource != null && !voiceaudioSource.isPlaying)
        {
            PlayNextRandomVoiceClip();
        }
    }

    // Полная остановка голоса/карандаша
    public static void StopVoice()
    {
        isVoiceLooping = false;
        if (voiceaudioSource != null)
        {
            voiceaudioSource.Stop();
            voiceaudioSource.clip = null;
        }
    }

    public static void SetVolume(float volume)
    {
        if (audioSource != null) audioSource.volume = volume;
        UpdateVoiceSourceVolume();
    }

    // Хелпер для правильного расчета громкости источника голоса/шагов
    private static void UpdateVoiceSourceVolume()
    {
        if (voiceaudioSource != null && Instance != null && Instance.sfxSlider != null)
        {
            // Итоговая громкость = значение ползунка * индивидуальный множитель звука
            voiceaudioSource.volume = Instance.sfxSlider.value * currentVolumeMultiplier;
        }
    }

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }
}
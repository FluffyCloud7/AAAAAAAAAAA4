using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // <-- Добавили для ползунка
using System;

public class SoundEffectLibrary : MonoBehaviour
{
    public static SoundEffectLibrary Instance { get; private set; }

    [SerializeField] private SoundEffectGroup[] soundEffectGroups;
    private Dictionary<string, List<AudioClip>> soundDictionary;

    // Хранилище громкости эффектов, переживающее смену сцен
    public float SfxVolume { get; private set; } = 1f;
    private Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод привязки ползунка звуковых эффектов из меню паузы
    public void BindSlider(Slider slider)
    {
        sfxSlider = slider;
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = SfxVolume; // Восстанавливаем положение ползунка звуков
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    private void SetSfxVolume(float volume)
    {
        SfxVolume = volume;
        // Здесь можно дополнительно управлять AudioMixer, если ты решишь его внедрить позже
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            soundDictionary[soundEffectGroup.name] = soundEffectGroup.audioClips;
        }
    }

    public AudioClip GetRandomClip(string name)
    {
        if (soundDictionary != null && soundDictionary.ContainsKey(name))
        {
            List<AudioClip> audioClips = soundDictionary[name];
            if (audioClips.Count > 0)
            {
                return audioClips[UnityEngine.Random.Range(0, audioClips.Count)];
            }
        }
        return null;
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;
    public List<AudioClip> audioClips;
}
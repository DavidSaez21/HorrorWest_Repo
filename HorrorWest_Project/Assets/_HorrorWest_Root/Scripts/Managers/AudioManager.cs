using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public enum AudioManagerMode { Full, SFXOnly, MusicOnly }

    [Header("Modo")]
    public AudioManagerMode mode = AudioManagerMode.Full;

    [Header("SFX")]
    public AudioSource sfxSource;
    public Slider sfxSlider;

    [Header("Música")]
    public AudioSource musicSource;
    public Slider musicSlider;
    public AudioClip musicClip;

    void Start()
    {
        if (mode == AudioManagerMode.SFXOnly || mode == AudioManagerMode.Full)
        {
            ApplySFXVolume();
            InitSFXSlider();
        }

        if (mode == AudioManagerMode.MusicOnly || mode == AudioManagerMode.Full)
        {
            ApplyMusicVolume();
            InitMusicSlider();

            if (musicSource != null && musicClip != null)
            {
                musicSource.clip = musicClip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }

    void OnEnable()
    {
        if (mode == AudioManagerMode.SFXOnly || mode == AudioManagerMode.Full)
        {
            ApplySFXVolume();
            InitSFXSlider();
        }

        if (mode == AudioManagerMode.MusicOnly || mode == AudioManagerMode.Full)
        {
            ApplyMusicVolume();
            InitMusicSlider();
        }
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = PlayerPrefs.GetFloat("sfxVolume", 1f);
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
    }

    private void InitSFXSlider()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfxVolume", 1f));
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void InitMusicSlider()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("musicVolume", 1f));
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (sfxSource != null) sfxSource.volume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();

        if (musicSource != null)
        {
            musicSource.volume = value;
        }
        else
        {
            AudioManager[] managers = FindObjectsByType<AudioManager>(FindObjectsSortMode.None);
            foreach (AudioManager m in managers)
            {
                if (m != this && m.musicSource != null)
                    m.musicSource.volume = value;
            }
        }
    }

    public float GetSFXVolume() => PlayerPrefs.GetFloat("sfxVolume", 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat("musicVolume", 1f);
}
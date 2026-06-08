using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    public Slider sfxSlider;
    public AudioSource sfxSource;
    private float _sfxVolume = 1f;

    [Header("Música")]
    public Slider musicSlider;
    public AudioSource musicSource;
    private float _musicVolume = 1f;

    void Start()
    {
        // SFX
        float savedSFX = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolume = savedSFX;

        if (sfxSource != null)
            sfxSource.volume = savedSFX;

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(savedSFX);
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Música
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);
        _musicVolume = savedMusic;

        if (musicSource != null)
            musicSource.volume = savedMusic;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.SetValueWithoutNotify(savedMusic);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        _sfxVolume = value;
        if (sfxSource != null)
            sfxSource.volume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        _musicVolume = value;
        if (musicSource != null)
            musicSource.volume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("sfxVolume", 1f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("musicVolume", 1f);
    }
}
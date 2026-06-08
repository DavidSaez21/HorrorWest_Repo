using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    public Slider sfxSlider;
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

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Música
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);
        _musicVolume = savedMusic;

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = savedMusic;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (musicSource != null)
            musicSource.volume = _musicVolume;
    }

    public void OnSFXVolumeChanged(float value)
    {
        _sfxVolume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        _musicVolume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();

        if (musicSource != null)
            musicSource.volume = value;
    }

    public float GetSFXVolume()
    {
        return _sfxVolume;
    }

    public float GetMusicVolume()
    {
        return _musicVolume;
    }
}
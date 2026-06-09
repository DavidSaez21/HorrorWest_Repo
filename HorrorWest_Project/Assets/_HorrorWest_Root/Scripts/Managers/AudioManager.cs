using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    public AudioSource sfxSource;

    [Header("Música")]
    public AudioSource musicSource;

    [Header("Sliders UI (solo en escenas con Canvas)")]
    public Slider sfxSlider;
    public Slider musicSlider;

    void Start()
    {
        ApplySavedVolumes();
        InitSliders();
    }

    void OnEnable()
    {
        ApplySavedVolumes();
        InitSliders();
    }

    private void ApplySavedVolumes()
    {
        float savedSFX = PlayerPrefs.GetFloat("sfxVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);

        if (sfxSource != null) sfxSource.volume = savedSFX;
        if (musicSource != null) musicSource.volume = savedMusic;
    }

    private void InitSliders()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfxVolume", 1f));
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

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
        if (musicSource != null) musicSource.volume = value;
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save();
    }

    public float GetSFXVolume() => PlayerPrefs.GetFloat("sfxVolume", 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat("musicVolume", 1f);
}
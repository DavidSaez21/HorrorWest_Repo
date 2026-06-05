using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Slider sfxSlider;
    private float _sfxVolume = 1f;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        _sfxVolume = savedVolume;

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = savedVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        Debug.Log("Volumen cambiado a: " + value);
        _sfxVolume = value;
        PlayerPrefs.SetFloat("sfxVolume", value);
        PlayerPrefs.Save();
    }

    public float GetSFXVolume()
    {
        return _sfxVolume;
    }
}
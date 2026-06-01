using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    public AudioSource audioSource;
    public AudioManager audioManager;
    [Range(0f, 1f)]
    public float volume = 1f;

    public void PlayClick()
    {
        if (clickSound != null && audioSource != null)
        {
            float sfxVolume = audioManager != null ? audioManager.GetSFXVolume() : 1f;
            audioSource.PlayOneShot(clickSound, volume * sfxVolume);
        }
    }
}
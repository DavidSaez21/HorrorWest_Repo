using UnityEngine;
using System.Collections;

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
            float sfxVolume = audioManager != null ? audioManager.GetSFXVolume() : PlayerPrefs.GetFloat("sfxVolume", 1f);
            audioSource.PlayOneShot(clickSound, volume * sfxVolume);
        }
    }

    public void PlayClickAndLoad(string sceneName)
    {
        StartCoroutine(LoadAfterSound(sceneName));
    }

    private IEnumerator LoadAfterSound(string sceneName)
    {
        PlayClick();
        yield return new WaitForSeconds(clickSound != null ? clickSound.length : 0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
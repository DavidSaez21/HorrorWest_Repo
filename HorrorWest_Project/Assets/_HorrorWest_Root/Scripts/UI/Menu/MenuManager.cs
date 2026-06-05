using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;
    public ButtonSound buttonSound;

    public void ClickPlay()
    {
        SceneManager.LoadScene("SCN_LVL1");
    }

    public void ClickQuit()
    {
        Debug.Log("Has cerrado el juego");
    }

    public void ClickOptions()
    {
        panelOptions.SetActive(true);
    }

    public void CloseOptions()
    {
        panelOptions.SetActive(false);
        panelControls.SetActive(false);
        panelAudio.SetActive(false);
    }

    public void ClickControls()
    {
        panelControls.SetActive(true);
        panelAudio.SetActive(false);
    }

    public void CloseControls()
    {
        panelControls.SetActive(false);
    }

    public void ClickAudio()
    {
        panelControls.SetActive(false);
        panelAudio.SetActive(true);
    }

    public void CloseAudio()
    {
        panelAudio.SetActive(false);
    }

    public void ClickPlayFromShop()
    {
        StartCoroutine(LoadAfterSound("SCN_LVL1"));
    }

    public void ClickBackToMenu()
    {
        StartCoroutine(LoadAfterSound("SCN_MainMenu"));
    }

    private IEnumerator LoadAfterSound(string sceneName)
    {
        if (buttonSound != null)
        {
            buttonSound.PlayClick();
            yield return new WaitForSeconds(buttonSound.clickSound.length);
        }
        SceneManager.LoadScene(sceneName);
    }
}
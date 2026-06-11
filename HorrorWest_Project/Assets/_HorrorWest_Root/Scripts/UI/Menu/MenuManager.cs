using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;
    public ButtonSound buttonSound;
    public WesternSignSwing btnPlay;
    public WesternSignSwing btnOptions;
    public WesternSignSwing btnExit;
    public WesternSignSwing btnControls;
    public WesternSignSwing btnAudio;
    public WesternSignSwing btnCloseOptions;

    private void SetMainButtonsActive(bool active)
    {
        if (btnPlay != null) btnPlay.enabled = active;
        if (btnOptions != null) btnOptions.enabled = active;
        if (btnExit != null) btnExit.enabled = active;
    }

    private void SetOptionsButtonsActive(bool active)
    {
        if (btnControls != null) btnControls.enabled = active;
        if (btnAudio != null) btnAudio.enabled = active;
        if (btnCloseOptions != null) btnCloseOptions.enabled = active;
    }

    public void ClickPlay()
    {
        if (PlayerPrefs.GetInt("HasPlayed", 0) == 0)
        {
            PlayerPrefs.SetInt("HasPlayed", 1);
            PlayerPrefs.Save();
            StartCoroutine(LoadAfterSound("SCN_S1"));
        }
        else
        {
            StartCoroutine(LoadAfterSound("SCN_Tienda"));
        }
    }

    public void ClickQuit()
    {
        Application.Quit();
        Debug.Log("Has cerrado el juego");
    }

    public void ClickOptions()
    {
        panelOptions.SetActive(true);
        SetMainButtonsActive(false);
    }

    public void CloseOptions()
    {
        panelOptions.SetActive(false);
        panelControls.SetActive(false);
        panelAudio.SetActive(false);
        SetMainButtonsActive(true);
    }

    public void ClickControls()
    {
        panelControls.SetActive(true);
        panelAudio.SetActive(false);
        SetOptionsButtonsActive(false);
    }

    public void CloseControls()
    {
        panelControls.SetActive(false);
        SetOptionsButtonsActive(true);
    }

    public void ClickAudio()
    {
        panelControls.SetActive(false);
        panelAudio.SetActive(true);
        SetOptionsButtonsActive(false);
    }

    public void CloseAudio()
    {
        panelAudio.SetActive(false);
        SetOptionsButtonsActive(true);
    }

    public void ClickPlayFromShop()
    {
        StartCoroutine(LoadAfterSound("SCN_LVL1"));
    }

    public void ClickBackToMenu()
    {
        StartCoroutine(LoadAfterSound("SCN_MainMenu"));
    }

    public void ClickRestart()
    {
        SceneManager.LoadScene("SCN_LVL1");
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
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;

    public void ClickPlay()
    {
        SceneManager.LoadScene("Test_Pausa");
    }

    public void ClickQuit()
    {
        Debug.Log("Boton quit pulsado");
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
        SceneManager.LoadScene("Test_Pausa");
    }

    public void ClickBackToMenu()
    {
        SceneManager.LoadScene("SCN_MainMenu");
    }
}
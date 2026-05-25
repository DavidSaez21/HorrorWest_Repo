using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;

    public void ClickPlay()
    {
        Debug.Log("Boton play pulsado");
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
}

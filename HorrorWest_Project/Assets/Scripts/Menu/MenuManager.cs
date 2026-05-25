using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOptions;

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
}

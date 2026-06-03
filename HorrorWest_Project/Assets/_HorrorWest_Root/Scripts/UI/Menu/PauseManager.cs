using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject panelPause;
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;

    private bool _isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        panelPause.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void Resume()
    {
        panelPause.SetActive(false);
        if (panelOptions != null) panelOptions.SetActive(false);
        if (panelControls != null) panelControls.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;
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

    public void Surrender()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SCN_Tienda");
    }
}
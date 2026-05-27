using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject panelPause;
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
        Debug.Log("Juego pausado");
    }

    public void Resume()
    {
        panelPause.SetActive(false);
        Time.timeScale = 1f; 
        _isPaused = false;
        Debug.Log("Juego reanudado");
    }

    public void Surrender()
    {
        Time.timeScale = 1f; 
        Debug.Log("Rendirse pulsado");
        // Aqui mas adelante cargaremos la escena del menu principal
    }
}

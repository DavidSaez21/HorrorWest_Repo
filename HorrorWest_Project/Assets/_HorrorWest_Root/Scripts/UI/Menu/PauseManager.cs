using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject panelPause;
    public GameObject panelOptions;
    public GameObject panelControls;
    public GameObject panelAudio;

    private bool _isPaused = false;
    private PlayerInput _playerInput;

    void Awake()
    {
        if (FindObjectsOfType<PauseManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (_playerInput == null)
            _playerInput = FindFirstObjectByType<PlayerInput>();

        panelPause.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;

        if (_playerInput != null) _playerInput.DeactivateInput();
    }

    public void Resume()
    {
        if (_playerInput == null)
            _playerInput = FindFirstObjectByType<PlayerInput>();

        panelPause.SetActive(false);
        if (panelOptions != null) panelOptions.SetActive(false);
        if (panelControls != null) panelControls.SetActive(false);
        if (panelAudio != null) panelAudio.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;

        if (_playerInput != null) _playerInput.ActivateInput();
    }

    public void ClickOptions() { panelOptions.SetActive(true); }

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

    public void CloseControls() { panelControls.SetActive(false); }

    public void ClickAudio()
    {
        panelControls.SetActive(false);
        panelAudio.SetActive(true);
    }

    public void CloseAudio() { panelAudio.SetActive(false); }

    public void Surrender()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SCN_Tienda");
    }
}
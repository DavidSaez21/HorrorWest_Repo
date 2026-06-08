using UnityEngine;

public class BarEntrance : MonoBehaviour
{
    [Header("Calle")]
    [SerializeField] private GameObject streetTilemap;
    [SerializeField] private GameObject streetProps;
    [SerializeField] private WaveManager streetWaveManager;

    [Header("Bloqueador entrada (se quita al limpiar la calle)")]
    [SerializeField] private GameObject streetDoorBlocker;

    [Header("Bar / Interior")]
    [SerializeField] private GameObject barTilemap;
    [SerializeField] private GameObject barProps;
    [SerializeField] private Transform playerSpawnInside;
    [SerializeField] private GameObject doorBlocker;
    [SerializeField] private WaveManager barWaveManager;
    [SerializeField] private GameObject sceneChangeTrigger;

    [Header("Al salir del interior (solo cárcel)")]
    [SerializeField] private bool reactivateStreetOnClear = false;

    [Header("Confiner calle")]
    [SerializeField] private Vector2 streetCamMin;
    [SerializeField] private Vector2 streetCamMax;

    [Header("Confiner interior")]
    [SerializeField] private Vector2 barCamMin;
    [SerializeField] private Vector2 barCamMax;

    private bool _insideBar = false;
    private bool _streetCleared = false;
    private bool _barCleared = false;
    private CameraConfiner _confiner;

    private void Start()
    {
        if (barTilemap != null) barTilemap.SetActive(false);
        if (barProps != null) barProps.SetActive(false);
        if (doorBlocker != null) doorBlocker.SetActive(false);
        if (streetDoorBlocker != null) streetDoorBlocker.SetActive(true);
        if (sceneChangeTrigger != null) sceneChangeTrigger.SetActive(false);

        _confiner = Camera.main.GetComponent<CameraConfiner>();
        if (_confiner != null)
            _confiner.SetBounds(streetCamMin, streetCamMax);

        if (streetWaveManager != null)
            streetWaveManager.OnAllWavesCompleted += OnStreetCleared;
    }

    private void OnStreetCleared()
    {
        _streetCleared = true;
        if (streetDoorBlocker != null) streetDoorBlocker.SetActive(false);
    }

    public void EnterBar()
    {
        if (_insideBar) return;
        if (!_streetCleared) return;

        _insideBar = true;

        if (streetTilemap != null) streetTilemap.SetActive(false);
        if (streetProps != null) streetProps.SetActive(false);

        if (barTilemap != null) barTilemap.SetActive(true);
        if (barProps != null) barProps.SetActive(true);

        if (doorBlocker != null) doorBlocker.SetActive(true);

        if (playerSpawnInside != null)
            PlayerManager.Instance.transform.position = playerSpawnInside.position;

        if (_confiner != null)
            _confiner.SetBounds(barCamMin, barCamMax);

        if (barWaveManager != null)
        {
            barWaveManager.OnAllWavesCompleted += OnBarCleared;
            barWaveManager.StartWaves();
        }
    }

    private void OnBarCleared()
    {
        _barCleared = true;

        // Solo desbloquea la puerta — el player sale manualmente
        if (doorBlocker != null) doorBlocker.SetActive(false);

        if (barWaveManager != null)
            barWaveManager.OnAllWavesCompleted -= OnBarCleared;
    }

    // Llamar desde el trigger de salida de la puerta
    public void ExitInterior()
    {
        if (!_barCleared) return;

        if (reactivateStreetOnClear)
        {
            if (barTilemap != null) barTilemap.SetActive(false);
            if (barProps != null) barProps.SetActive(false);

            if (streetTilemap != null) streetTilemap.SetActive(true);
            if (streetProps != null) streetProps.SetActive(true);

            if (_confiner != null)
                _confiner.SetBounds(streetCamMin, streetCamMax);

            // Relanzar oleadas de la calle
            if (streetWaveManager != null)
            {
                streetWaveManager.ResetWaves();
                streetWaveManager.StartWaves();
            }
        }

        if (sceneChangeTrigger != null) sceneChangeTrigger.SetActive(true);
    }

    private void OnDestroy()
    {
        if (streetWaveManager != null)
            streetWaveManager.OnAllWavesCompleted -= OnStreetCleared;
    }
}
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

    [Header("Música")]
    [SerializeField] private AudioClip barMusic;

    private bool _insideBar = false;
    private bool _streetCleared = false;
    private bool _barCleared = false;
    private CameraConfiner _confiner;
    private AudioManager _audioManager;
    private AudioClip _streetMusic;

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

        _audioManager = FindFirstObjectByType<AudioManager>();
        if (_audioManager != null && _audioManager.musicSource != null)
            _streetMusic = _audioManager.musicSource.clip;
    }

    private void OnStreetCleared()
    {
        _streetCleared = true;
        if (streetDoorBlocker != null) streetDoorBlocker.SetActive(false);

        if (barWaveManager == null && sceneChangeTrigger != null)
            sceneChangeTrigger.SetActive(true);
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

        if (_audioManager != null && barMusic != null)
        {
            _audioManager.musicSource.clip = barMusic;
            _audioManager.musicSource.Play();
        }
    }

    private void OnBarCleared()
    {
        _barCleared = true;

        if (doorBlocker != null) doorBlocker.SetActive(false);

        if (!reactivateStreetOnClear)
        {
            if (sceneChangeTrigger != null) sceneChangeTrigger.SetActive(true);
        }

        if (barWaveManager != null)
            barWaveManager.OnAllWavesCompleted -= OnBarCleared;
    }

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

            if (streetWaveManager != null)
            {
                streetWaveManager.ResetWaves();
                streetWaveManager.StartWaves();
            }

            if (_audioManager != null && _streetMusic != null)
            {
                _audioManager.musicSource.clip = _streetMusic;
                _audioManager.musicSource.Play();
            }
        }

        if (sceneChangeTrigger != null) sceneChangeTrigger.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        ExitInterior();
    }

    private void OnDestroy()
    {
        if (streetWaveManager != null)
            streetWaveManager.OnAllWavesCompleted -= OnStreetCleared;
    }
}
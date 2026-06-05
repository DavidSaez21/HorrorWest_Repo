using UnityEngine;

public class BarEntrance : MonoBehaviour
{
    [Header("Calle")]
    [SerializeField] private GameObject streetTilemap;
    [SerializeField] private GameObject streetProps;

    [Header("Bar / Interior")]
    [SerializeField] private GameObject barTilemap;
    [SerializeField] private GameObject barProps;
    [SerializeField] private Transform playerSpawnInside;
    [SerializeField] private GameObject doorBlocker;

    [Header("Confiner calle")]
    [SerializeField] private Vector2 streetCamMin;
    [SerializeField] private Vector2 streetCamMax;

    [Header("Confiner interior")]
    [SerializeField] private Vector2 barCamMin;
    [SerializeField] private Vector2 barCamMax;

    private bool _insideBar = false;
    private CameraConfiner _confiner;

    private void Start()
    {
        barTilemap.SetActive(false);
        barProps.SetActive(false);
        if (doorBlocker != null) doorBlocker.SetActive(false);

        _confiner = Camera.main.GetComponent<CameraConfiner>();

        // Activa el confiner del pueblo al inicio
        if (_confiner != null)
            _confiner.SetBounds(streetCamMin, streetCamMax);
    }

    public void EnterBar()
    {
        if (_insideBar) return;
        _insideBar = true;

        streetTilemap.SetActive(false);
        streetProps.SetActive(false);

        barTilemap.SetActive(true);
        barProps.SetActive(true);

        if (doorBlocker != null) doorBlocker.SetActive(true);

        if (playerSpawnInside != null)
            PlayerManager.Instance.transform.position = playerSpawnInside.position;

        // Cambia al confiner del interior
        if (_confiner != null)
            _confiner.SetBounds(barCamMin, barCamMax);
    }
}
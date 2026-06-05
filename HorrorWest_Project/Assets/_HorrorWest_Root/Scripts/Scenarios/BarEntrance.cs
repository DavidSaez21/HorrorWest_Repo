using UnityEngine;

public class BarEntrance : MonoBehaviour
{
    [Header("Calle")]
    [SerializeField] private GameObject streetTilemap;
    [SerializeField] private GameObject streetProps;

    [Header("Bar")]
    [SerializeField] private GameObject barTilemap;
    [SerializeField] private GameObject barProps;
    [SerializeField] private Transform playerSpawnInside;
    [SerializeField] private GameObject doorBlocker;

    [Header("Límites cámara bar")]
    [SerializeField] private Vector2 barCamMin;   // esquina inf izq del bar
    [SerializeField] private Vector2 barCamMax;   // esquina sup der del bar

    private bool _insideBar = false;
    private CameraConfiner _confiner;

    private void Start()
    {
        barTilemap.SetActive(false);
        barProps.SetActive(false);
        if (doorBlocker != null) doorBlocker.SetActive(false);

        _confiner = Camera.main.GetComponent<CameraConfiner>();
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

        if (_confiner != null)
            _confiner.SetBounds(barCamMin, barCamMax);
    }
}
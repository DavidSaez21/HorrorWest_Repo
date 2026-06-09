using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomCursor : MonoBehaviour
{
    [Header("Visual")]
    public RectTransform cursorVisual;
    public Vector2 hotspot = Vector2.zero;
    public Image cursorImage;

    [Header("Sprites")]
    public Sprite crosshairSprite;
    public Sprite handSprite;

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;

    [Header("Escenas de juego")]
    public string[] gameplayScenes;

    private bool _isGameplayScene = false;
    private bool _forcedHand = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _forcedHand = false;
        UpdateCursorForScene(scene.name);
    }

    void Start()
    {
        Cursor.visible = false;
        UpdateCursorForScene(SceneManager.GetActiveScene().name);
    }

    void UpdateCursorForScene(string sceneName)
    {
        _isGameplayScene = System.Array.Exists(gameplayScenes, s => s == sceneName);
        cursorImage.sprite = _isGameplayScene ? crosshairSprite : handSprite;
        cursorImage.color = normalColor;
    }

    public void SetHandCursor()
    {
        _forcedHand = true;
        cursorImage.sprite = handSprite;
        cursorImage.color = normalColor;
    }

    public void RestoreCursor()
    {
        _forcedHand = false;
        UpdateCursorForScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        cursorVisual.position = new Vector3(mousePos.x - hotspot.x, mousePos.y - hotspot.y, 0);

        if (_forcedHand || !_isGameplayScene || Camera.main == null) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.CompareTag("Enemy"))
            cursorImage.color = enemyColor;
        else
            cursorImage.color = normalColor;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }
}
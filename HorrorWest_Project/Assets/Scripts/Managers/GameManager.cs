using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ─────────────────────────────────────────────────────────────────────────────
// GameManager — cerebro de la run.
//
// Responsabilidades:
//   · Estado global de la partida (jugando, pausado, muerto, completado)
//   · Coordinación entre niveles (qué nivel estamos, cuándo pasar al siguiente)
//   · Pausa (absorbe PauseManager)
//   · Muerte del jugador → transición a tienda
//   · Inicio de run → reset de stats y pool de mejoras
//
// NO gestiona:
//   · Las oleadas dentro de un nivel (eso sigue siendo WaveManager)
//   · Las stats del jugador (PlayerManager)
//   · La economía (CurrencyManager)
//
// ESCENA: vive en la escena del nivel como GameObject vacío "GameManager".
// ─────────────────────────────────────────────────────────────────────────────
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Configuración de niveles ──────────────────────────────────────────────
    [Header("Level Settings")]
    [Tooltip("Nombres de las escenas de cada nivel en orden")]
    [SerializeField] private string[] levelScenes = { "Level_1", "Level_2", "Level_3", "Level_4" };
    [SerializeField] private string shopScene = "SCN_Shop";
    [SerializeField] private string menuScene = "SCN_Menu";

    [Header("Transition Settings")]
    [SerializeField] private float deathToShopDelay = 1.5f;   // Segundos tras morir antes de ir a la tienda
    [SerializeField] private float levelCompleteDelay = 2f;     // Segundos tras completar nivel antes de ir al siguiente

    // ── UI ────────────────────────────────────────────────────────────────────
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject levelCompletePanel;

    // ── Estado de la run ──────────────────────────────────────────────────────
    public int CurrentLevel { get; private set; } = 0;
    public bool IsPaused { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;
    public bool IsRunActive { get; private set; } = false;

    // ── Eventos ───────────────────────────────────────────────────────────────
    public event System.Action OnRunStarted;
    public event System.Action OnPlayerDied;
    public event System.Action<int> OnLevelChanged;     // índice del nuevo nivel
    public event System.Action OnRunCompleted;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Detecta en qué nivel estamos según la escena activa
        CurrentLevel = GetCurrentLevelIndex();
        StartRun();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    // ── Inicio de run ─────────────────────────────────────────────────────────
    private void StartRun()
    {
        IsRunActive = true;
        IsGameOver = false;
        Time.timeScale = 1f;

        // Suscribirse a la muerte del jugador
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.Health.OnDeath += HandlePlayerDeath;

        // Suscribirse al WaveManager para saber cuándo se completa el nivel
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.OnAllWavesCompleted += HandleLevelCompleted;

        // Reset de stats solo en el primer nivel de la run
        if (CurrentLevel == 0)
        {
            PlayerManager.Instance?.ResetForNewRun();
            UpgradePool.Instance?.ResetPool();
        }

        OnRunStarted?.Invoke();

        #region Debug
        Debug.Log($"[GameManager] Run iniciada. Nivel: {CurrentLevel + 1} / {levelScenes.Length}");
        #endregion
    }

    // ── Muerte del jugador ────────────────────────────────────────────────────
    private void HandlePlayerDeath()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsRunActive = false;

        OnPlayerDied?.Invoke();

        #region Debug
        Debug.Log("[GameManager] Jugador muerto. Cargando tienda...");
        #endregion

        StartCoroutine(GoToShopAfterDelay());
    }

    private IEnumerator GoToShopAfterDelay()
    {
        yield return new WaitForSeconds(deathToShopDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(shopScene);
    }

    // ── Nivel completado ──────────────────────────────────────────────────────
    private void HandleLevelCompleted()
    {
        #region Debug
        Debug.Log($"[GameManager] Nivel {CurrentLevel + 1} completado.");
        #endregion

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        StartCoroutine(GoToNextLevelAfterDelay());
    }

    private IEnumerator GoToNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(levelCompleteDelay);

        int nextLevel = CurrentLevel + 1;

        if (nextLevel >= levelScenes.Length)
        {
            // Run completada — el jugador ha pasado los 4 niveles
            IsRunActive = false;
            OnRunCompleted?.Invoke();

            #region Debug
            Debug.Log("[GameManager] ¡Run completada!");
            #endregion

            // Por ahora vuelve al menú; aquí puedes poner una pantalla de victoria
            SceneManager.LoadScene(menuScene);
        }
        else
        {
            OnLevelChanged?.Invoke(nextLevel);
            SceneManager.LoadScene(levelScenes[nextLevel]);
        }
    }

    // ── Pausa (absorbe PauseManager) ─────────────────────────────────────────
    public void Pause()
    {
        if (IsGameOver) return;
        IsPaused = true;
        Time.timeScale = 0f;
        pausePanel?.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        pausePanel?.SetActive(false);
        optionsPanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        audioPanel?.SetActive(false);
    }

    // ── Botones UI de pausa ───────────────────────────────────────────────────
    public void ClickOptions() => optionsPanel?.SetActive(true);
    public void CloseOptions()
    {
        optionsPanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        audioPanel?.SetActive(false);
    }
    public void ClickControls()
    {
        controlsPanel?.SetActive(true);
        audioPanel?.SetActive(false);
    }
    public void CloseControls() => controlsPanel?.SetActive(false);

    public void ClickAudio()
    {
        audioPanel?.SetActive(true);
        controlsPanel?.SetActive(false);
    }
    public void CloseAudio() => audioPanel?.SetActive(false);

    /// <summary>Botón Rendirse — vuelve al menú principal.</summary>
    public void Surrender()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Detecta en qué nivel estamos comparando el nombre de la escena activa
    /// con el array levelScenes.
    /// </summary>
    private int GetCurrentLevelIndex()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == currentScene)
                return i;
        }
        return 0;
    }

    private void OnDestroy()
    {
        // Limpia suscripciones al destruirse
        if (PlayerManager.Instance?.Health != null)
            PlayerManager.Instance.Health.OnDeath -= HandlePlayerDeath;
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    [ContextMenu("Debug — Simular muerte del jugador")]
    private void DebugSimulateDeath() => HandlePlayerDeath();

    [ContextMenu("Debug — Simular nivel completado")]
    private void DebugSimulateLevelComplete() => HandleLevelCompleted();

    [ContextMenu("Debug — Imprimir estado")]
    private void DebugPrintState()
    {
        Debug.Log($"[GameManager] Nivel: {CurrentLevel + 1} | Pausado: {IsPaused} | GameOver: {IsGameOver} | RunActiva: {IsRunActive}");
    }
    #endregion
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private string[] levelScenes = { "Level_1", "Level_2", "Level_3", "Level_4" };
    [SerializeField] private string shopScene = "SCN_Shop";
    [SerializeField] private string menuScene = "SCN_Menu";

    [Header("Transition Settings")]
    [SerializeField] private float deathToShopDelay = 1.5f;
    [SerializeField] private float levelCompleteDelay = 2f;

    public int CurrentLevel { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsRunActive { get; private set; } = false;

    public event System.Action OnRunStarted;
    public event System.Action OnPlayerDied;
    public event System.Action<int> OnLevelChanged;
    public event System.Action OnRunCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        CurrentLevel = GetCurrentLevelIndex();
        Debug.Log($"[GameManager] Escena: {SceneManager.GetActiveScene().name} — Nivel detectado: {CurrentLevel}");
        StartRun();
    }

    private void StartRun()
    {
        IsRunActive = true;
        IsGameOver = false;
        Time.timeScale = 1f;

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.Health.OnDeath += HandlePlayerDeath;

        if (CurrentLevel == 0)
        {
            // Run nueva — resetea todo
            PlayerManager.Instance?.ResetForNewRun();
            UpgradePool.Instance?.ResetPool();
        }
        else
        {
            // Nivel siguiente — re-registra únicas para que no vuelvan a salir
            PlayerManager.Instance?.Stats?.ReRegisterUniqueUpgrades();
        }

        // Curación al inicio de escena (vendas)
        if (PermanentUpgradeManager.Instance != null && PlayerManager.Instance != null)
        {
            float healAmount = PermanentUpgradeManager.Instance.GetHealOnRunStart();
            if (healAmount > 0f)
                PlayerManager.Instance.Heal(healAmount);
        }

        // Regeneración pasiva (pipa)
        if (PermanentUpgradeManager.Instance != null)
        {
            float regenPerSecond = PermanentUpgradeManager.Instance.GetHealthRegenPerSecond();
            if (regenPerSecond > 0f)
                StartCoroutine(HealthRegenRoutine(regenPerSecond));
        }

        OnRunStarted?.Invoke();
        Debug.Log($"[GameManager] Run iniciada. Nivel: {CurrentLevel + 1} / {levelScenes.Length}");
    }

    private IEnumerator HealthRegenRoutine(float regenPerSecond)
    {
        while (IsRunActive && !IsGameOver)
        {
            yield return new WaitForSeconds(1f);
            PlayerManager.Instance?.Heal(regenPerSecond);
        }
    }

    private void HandlePlayerDeath()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        IsRunActive = false;
        OnPlayerDied?.Invoke();
        Debug.Log("[GameManager] Jugador muerto. Cargando tienda...");
        StartCoroutine(GoToShopAfterDelay());
    }

    private IEnumerator GoToShopAfterDelay()
    {
        yield return new WaitForSeconds(deathToShopDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(shopScene);
    }

    private void HandleLevelCompleted()
    {
        Debug.Log($"[GameManager] Nivel {CurrentLevel + 1} completado.");
        StartCoroutine(GoToNextLevelAfterDelay());
    }

    private IEnumerator GoToNextLevelAfterDelay()
    {
        yield return new WaitForSeconds(levelCompleteDelay);
        int nextLevel = CurrentLevel + 1;
        if (nextLevel >= levelScenes.Length)
        {
            IsRunActive = false;
            OnRunCompleted?.Invoke();
            SceneManager.LoadScene(menuScene);
        }
        else
        {
            OnLevelChanged?.Invoke(nextLevel);
            SceneManager.LoadScene(levelScenes[nextLevel]);
        }
    }

    public void Surrender()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    private int GetCurrentLevelIndex()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        for (int i = 0; i < levelScenes.Length; i++)
            if (levelScenes[i] == currentScene) return i;
        return 0;
    }

    private void OnDestroy()
    {
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
    #endregion
}
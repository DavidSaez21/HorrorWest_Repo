using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// PlayerManager — punto de acceso central a todos los componentes del jugador.
//
// PROBLEMA QUE RESUELVE:
//   Antes cada sistema tenía su propio singleton (PlayerStats.Instance,
//   PlayerHealth.Instance, PlayerAiming.Instance...) y cualquier script
//   externo tenía que conocer y buscar cada uno por separado.
//
// AHORA:
//   Un solo punto de entrada: PlayerManager.Instance.Stats, .Health, etc.
//   Los singletons individuales de Stats, Health y Aiming se eliminan.
//   PlayerManager vive en el prefab del jugador.
//
// NOTA PARA EL EQUIPO:
//   Si un script externo necesita hacer daño al jugador:
//     PlayerManager.Instance.Health.TakeDamage(10f);
//   Si necesita saber si está apuntando:
//     PlayerManager.Instance.Aiming.IsAiming;
//   Si necesita aplicar una mejora:
//     PlayerManager.Instance.Stats.ApplyUpgrade(data);
// ─────────────────────────────────────────────────────────────────────────────
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    // ── Referencias a componentes del jugador ─────────────────────────────────
    [Header("Player Components")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerHealth health;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAiming aiming;
    [SerializeField] private PlayerShoot shoot;

    // ── Acceso público (read-only) ────────────────────────────────────────────
    public PlayerStats Stats => stats;
    public PlayerHealth Health => health;
    public PlayerMovement Movement => movement;
    public PlayerAiming Aiming => aiming;
    public PlayerShoot Shoot => shoot;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ResolveComponents();
    }

    /// <summary>
    /// Intenta obtener los componentes automáticamente si no están asignados
    /// en el Inspector. Evita errores silenciosos por referencias vacías.
    /// </summary>
    private void ResolveComponents()
    {
        if (stats == null) stats = GetComponentInChildren<PlayerStats>();
        if (health == null) health = GetComponentInChildren<PlayerHealth>();
        if (movement == null) movement = GetComponentInChildren<PlayerMovement>();
        if (aiming == null) aiming = GetComponentInChildren<PlayerAiming>();
        if (shoot == null) shoot = GetComponentInChildren<PlayerShoot>();

        #region Debug
        if (stats == null) Debug.LogError("[PlayerManager] PlayerStats no encontrado.");
        if (health == null) Debug.LogError("[PlayerManager] PlayerHealth no encontrado.");
        if (movement == null) Debug.LogError("[PlayerManager] PlayerMovement no encontrado.");
        if (aiming == null) Debug.LogError("[PlayerManager] PlayerAiming no encontrado.");
        if (shoot == null) Debug.LogError("[PlayerManager] PlayerShoot no encontrado.");
        #endregion
    }

    // ── Helpers de alto nivel ─────────────────────────────────────────────────
    // Métodos de conveniencia para las operaciones más comunes, evitando
    // que scripts externos tengan que encadenar PlayerManager.Instance.Health.X

    /// <summary>Aplica daño al jugador. Usado por enemigos y trampas.</summary>
    public void TakeDamage(float amount) => health?.TakeDamage(amount);

    /// <summary>Cura al jugador. Usado por mejoras y consumibles.</summary>
    public void Heal(float amount) => health?.Heal(amount);

    /// <summary>Aplica una mejora in-run. Usado por UpgradePanel.</summary>
    public void ApplyUpgrade(UpgradeData upgrade) => stats?.ApplyUpgrade(upgrade);

    /// <summary>Resetea stats al inicio de una nueva run.</summary>
    public void ResetForNewRun()
    {
        stats?.ResetStats();
        // GameManager llamará a esto al iniciar cada run
    }

    /// <summary>True si el jugador existe y está vivo.</summary>
    public bool IsAlive => health != null && health.GetCurrentHealth() > 0f;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugLogReferences = false;

    private void Start()
    {
        if (!debugLogReferences) return;
        Debug.Log($"[PlayerManager] Stats:    {(stats != null ? "OK" : "NULL")}");
        Debug.Log($"[PlayerManager] Health:   {(health != null ? "OK" : "NULL")}");
        Debug.Log($"[PlayerManager] Movement: {(movement != null ? "OK" : "NULL")}");
        Debug.Log($"[PlayerManager] Aiming:   {(aiming != null ? "OK" : "NULL")}");
        Debug.Log($"[PlayerManager] Shoot:    {(shoot != null ? "OK" : "NULL")}");
    }
    #endregion
}
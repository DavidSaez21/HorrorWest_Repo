using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weighted action queue for the Church Boss.
///
/// Design:
///   - Each attack has an independent cooldown (not a shared global cooldown).
///   - Weights are modified at runtime based on context (player position, active attacks, etc.).
///   - A configurable pressure budget caps how many attacks can be active simultaneously.
///   - Ground mouths are flagged as "background" attacks — they bypass the budget.
///   - The same attack cannot be selected twice in a row (weight drops to 0 for one cycle).
/// </summary>
public class BossActionQueue : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Pressure Budget")]
    [Tooltip("Maximum number of non-background attacks active at the same time. Tune in playtesting.")]
    [SerializeField] private int maxSimultaneousAttacks = 2;

    [Header("Tongue")]
    [SerializeField] private int tongueWeightBase = 3;
    [SerializeField] private float tongueCooldown = 8f;

    [Header("Tentacles")]
    [SerializeField] private int tentacleWeightBase = 4;
    [SerializeField] private float tentacleCooldown = 5f;

    [Header("Ground Mouths")]
    [SerializeField] private int mouthWeightBase = 3;
    [SerializeField] private float mouthCooldown = 10f;

    [Header("Minions")]
    [SerializeField] private int minionWeightBase = 2;
    [SerializeField] private float minionCooldown = 20f;

    [Header("Global Cooldown")]
    [Tooltip("Mandatory rest between attack cycles.")]
    [SerializeField] private float globalCooldownDuration = 1.5f;

    [Header("Context Thresholds")]
    [Tooltip("Distance from aisle centre at which the tongue weight bonus activates.")]
    [SerializeField] private float tongueDistanceCentreThreshold = 1.5f;
    [Tooltip("How long (seconds) the player must be nearly still to trigger the mouth weight bonus.")]
    [SerializeField] private float mouthStillThreshold = 2f;
    [Tooltip("Speed below which the player is considered 'still'.")]
    [SerializeField] private float playerStillSpeed = 0.5f;

    // ── Attack IDs ────────────────────────────────────────────────────────────

    public enum AttackId { Tongue, Tentacle, GroundMouth, Minion }

    // ── Runtime state ─────────────────────────────────────────────────────────

    // Last time each attack was launched
    private readonly Dictionary<AttackId, float> _lastUsed = new()
    {
        { AttackId.Tongue,      -999f },
        { AttackId.Tentacle,    -999f },
        { AttackId.GroundMouth, -999f },
        { AttackId.Minion,      -999f },
    };

    // Currently executing attacks
    private readonly HashSet<AttackId> _activeAttacks = new();

    // The last attack that was chosen (prevents consecutive repeats)
    private AttackId? _lastChosen = null;

    // Global cooldown timer
    private float _globalCooldownEnd = 0f;

    // Context tracking
    private Transform _player;
    private Vector3 _lastPlayerPos;
    private float _playerStillTimer = 0f;
    private bool _tongueActive = false; // suppresses tentacles during tongue lunge

    // ── External references (set by ChurchBoss) ───────────────────────────────

    private TongueAttack _tongue;
    private TentacleAttack _tentacle;
    private GroundMouthAttack _groundMouth;
    private MinionSpawner _minionSpawner;
    private BossEyeTracker _eyeTracker;

    // ── Initialisation ────────────────────────────────────────────────────────

    public void Initialise(
        Transform player,
        TongueAttack tongue,
        TentacleAttack tentacle,
        GroundMouthAttack groundMouth,
        MinionSpawner minionSpawner,
        BossEyeTracker eyeTracker)
    {
        _player = player;
        _tongue = tongue;
        _tentacle = tentacle;
        _groundMouth = groundMouth;
        _minionSpawner = minionSpawner;
        _eyeTracker = eyeTracker;
        _lastPlayerPos = player != null ? player.position : Vector3.zero;
    }

    // ── Main tick (called by ChurchBoss every Update) ─────────────────────────

    public void Tick()
    {
        if (_player == null) return;

        UpdateContextTracking();

        if (Time.time < _globalCooldownEnd) return;

        // Count active non-background attacks
        int activePressure = 0;
        foreach (var id in _activeAttacks)
            if (id != AttackId.GroundMouth) activePressure++;

        // Try to launch background attack independently (doesn't count toward budget)
        TryLaunchBackground();

        // Try to launch a foreground attack if budget allows
        if (activePressure < maxSimultaneousAttacks)
            TryLaunchForeground();
    }

    // ── Context tracking ──────────────────────────────────────────────────────

    private void UpdateContextTracking()
    {
        if (_player == null) return;

        float playerSpeed = Vector3.Distance(_player.position, _lastPlayerPos) / Time.deltaTime;
        _lastPlayerPos = _player.position;

        if (playerSpeed < playerStillSpeed)
            _playerStillTimer += Time.deltaTime;
        else
            _playerStillTimer = 0f;
    }

    // ── Attack selection ──────────────────────────────────────────────────────

    private void TryLaunchForeground()
    {
        // Build candidate list
        var candidates = new List<(AttackId id, int weight)>();

        TryAddCandidate(candidates, AttackId.Tongue,   ComputeTongueWeight(),   tongueCooldown);
        TryAddCandidate(candidates, AttackId.Tentacle, ComputeTentacleWeight(), tentacleCooldown);
        TryAddCandidate(candidates, AttackId.Minion,   ComputeMinionWeight(),   minionCooldown);

        if (candidates.Count == 0) return;

        AttackId chosen = WeightedRandom(candidates);
        Launch(chosen);
        _lastChosen = chosen;
        _globalCooldownEnd = Time.time + globalCooldownDuration;
    }

    private void TryLaunchBackground()
    {
        if (_activeAttacks.Contains(AttackId.GroundMouth)) return;
        if (!IsCooledDown(AttackId.GroundMouth, mouthCooldown)) return;
        if (_lastChosen == AttackId.GroundMouth) return; // no back-to-back

        int weight = ComputeMouthWeight();
        if (weight <= 0) return;

        Launch(AttackId.GroundMouth);
        _lastChosen = AttackId.GroundMouth;
    }

    private void TryAddCandidate(List<(AttackId, int)> list, AttackId id, int weight, float cooldown)
    {
        if (weight <= 0) return;
        if (_activeAttacks.Contains(id)) return;
        if (!IsCooledDown(id, cooldown)) return;
        if (_lastChosen == id) return; // no back-to-back
        list.Add((id, weight));
    }

    // ── Weight calculations (context-aware) ───────────────────────────────────

    private int ComputeTongueWeight()
    {
        if (_tongueActive) return 0;
        int w = tongueWeightBase;
        // Bonus if player has been near the centre of the aisle for a while
        if (_player != null && Mathf.Abs(_player.position.x) < tongueDistanceCentreThreshold)
            w += 2;
        return w;
    }

    private int ComputeTentacleWeight()
    {
        if (_tongueActive) return 0; // tongue suppresses tentacles
        int w = tentacleWeightBase;
        // Bonus if player is near a flank
        if (_player != null && Mathf.Abs(_player.position.x) > tongueDistanceCentreThreshold)
            w += 1;
        return w;
    }

    private int ComputeMouthWeight()
    {
        int w = mouthWeightBase;
        // Bonus if player has been still
        if (_playerStillTimer >= mouthStillThreshold) w += 1;
        return w;
    }

    private int ComputeMinionWeight()
    {
        int w = minionWeightBase;
        // Suppress if minions are already alive
        if (_minionSpawner != null && _minionSpawner.HasLivingMinions()) w -= 2;
        return Mathf.Max(0, w);
    }

    // ── Launching attacks ─────────────────────────────────────────────────────

    private void Launch(AttackId id)
    {
        _lastUsed[id] = Time.time;
        _activeAttacks.Add(id);

        // Telegraph: redirect eyes before executing
        _eyeTracker?.TelegraphAttack(id);

        switch (id)
        {
            case AttackId.Tongue:
                _tongueActive = true;
                _tongue?.Execute(() => OnAttackComplete(AttackId.Tongue));
                break;

            case AttackId.Tentacle:
                _tentacle?.Execute(() => OnAttackComplete(AttackId.Tentacle));
                break;

            case AttackId.GroundMouth:
                _groundMouth?.Execute(() => OnAttackComplete(AttackId.GroundMouth));
                break;

            case AttackId.Minion:
                _minionSpawner?.Execute(() => OnAttackComplete(AttackId.Minion));
                break;
        }
    }

    public void OnAttackComplete(AttackId id)
    {
        _activeAttacks.Remove(id);
        if (id == AttackId.Tongue) _tongueActive = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsCooledDown(AttackId id, float cooldown)
        => Time.time >= _lastUsed[id] + cooldown;

    private static AttackId WeightedRandom(List<(AttackId id, int weight)> candidates)
    {
        int total = 0;
        foreach (var (_, w) in candidates) total += w;

        int roll = Random.Range(0, total);
        int cumulative = 0;
        foreach (var (id, w) in candidates)
        {
            cumulative += w;
            if (roll < cumulative) return id;
        }
        return candidates[^1].id;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Immediately clears all active attacks. Call on boss death.</summary>
    public void StopAll()
    {
        _activeAttacks.Clear();
        _tongueActive = false;
    }

    public bool IsAttackActive(AttackId id) => _activeAttacks.Contains(id);

    // ── Debug gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Visualise the "aisle centre" threshold used for tongue/tentacle context
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireCube(transform.position,
            new Vector3(tongueDistanceCentreThreshold * 2f, 12f, 0f));
    }
}

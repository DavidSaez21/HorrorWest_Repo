using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weighted action queue for the Church Boss.
/// Cada ataque tiene peso, cooldown y presupuesto de presión independientes.
/// Todo configurable desde el Inspector del boss.
/// </summary>
public class BossActionQueue : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Pressure Budget")]
    [Tooltip("Máximo de ataques activos simultáneamente (sin contar bocas de suelo).")]
    [SerializeField] private int maxSimultaneousAttacks = 2;

    [Header("Tongue Bite")]
    [SerializeField] private int tongueBiteWeight = 3;
    [SerializeField] private float tongueBiteCooldown = 7f;

    [Header("Tongue Sweep")]
    [SerializeField] private int tongueSweepWeight = 2;
    [SerializeField] private float tongueSweepCooldown = 10f;

    [Header("Tentacles")]
    [SerializeField] private int tentacleWeight = 4;
    [SerializeField] private float tentacleCooldown = 5f;

    [Header("Ground Mouths")]
    [SerializeField] private int mouthWeight = 3;
    [SerializeField] private float mouthCooldown = 10f;

    [Header("Minions")]
    [SerializeField] private int minionWeight = 2;
    [SerializeField] private float minionCooldown = 20f;

    [Header("Global Cooldown")]
    [Tooltip("Pausa obligatoria entre ciclos de ataque.")]
    [SerializeField] private float globalCooldownDuration = 1.5f;

    [Header("Context Thresholds")]
    [SerializeField] private float tongueDistanceCentreThreshold = 1.5f;
    [SerializeField] private float mouthStillThreshold = 2f;
    [SerializeField] private float playerStillSpeed = 0.5f;

    // ── Attack IDs ────────────────────────────────────────────────────────────

    public enum AttackId { TongueBite, TongueSweep, Tentacle, GroundMouth, Minion }

    // ── Runtime ───────────────────────────────────────────────────────────────

    private readonly Dictionary<AttackId, float> _lastUsed = new()
    {
        { AttackId.TongueBite,  -999f },
        { AttackId.TongueSweep, -999f },
        { AttackId.Tentacle,    -999f },
        { AttackId.GroundMouth, -999f },
        { AttackId.Minion,      -999f },
    };

    private readonly HashSet<AttackId> _activeAttacks = new();
    private AttackId? _lastChosen = null;
    private float _globalCooldownEnd = 0f;

    private Transform _player;
    private Vector3 _lastPlayerPos;
    private float _playerStillTimer = 0f;

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

    // ── Main tick ─────────────────────────────────────────────────────────────

    public void Tick()
    {
        if (_player == null) return;

        UpdateContextTracking();

        if (Time.time < _globalCooldownEnd) return;

        // Contar ataques activos (bocas no cuentan)
        int activePressure = 0;
        foreach (var id in _activeAttacks)
            if (id != AttackId.GroundMouth) activePressure++;

        TryLaunchBackground();

        if (activePressure < maxSimultaneousAttacks)
            TryLaunchForeground();
    }

    // ── Context tracking ──────────────────────────────────────────────────────

    private void UpdateContextTracking()
    {
        if (_player == null) return;
        float speed = Vector3.Distance(_player.position, _lastPlayerPos) / Time.deltaTime;
        _lastPlayerPos = _player.position;
        _playerStillTimer = speed < playerStillSpeed
            ? _playerStillTimer + Time.deltaTime
            : 0f;
    }

    // ── Attack selection ──────────────────────────────────────────────────────

    private void TryLaunchForeground()
    {
        var candidates = new List<(AttackId id, int weight)>();
        TryAddCandidate(candidates, AttackId.TongueBite, ComputeTongueBiteWeight(), tongueBiteCooldown);
        TryAddCandidate(candidates, AttackId.TongueSweep, ComputeTongueSweepWeight(), tongueSweepCooldown);
        TryAddCandidate(candidates, AttackId.Tentacle, ComputeTentacleWeight(), tentacleCooldown);
        TryAddCandidate(candidates, AttackId.Minion, ComputeMinionWeight(), minionCooldown);

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
        if (_lastChosen == AttackId.GroundMouth) return;
        if (ComputeMouthWeight() <= 0) return;

        Launch(AttackId.GroundMouth);
        _lastChosen = AttackId.GroundMouth;
    }

    private void TryAddCandidate(List<(AttackId, int)> list, AttackId id, int weight, float cooldown)
    {
        if (weight <= 0) return;
        if (_activeAttacks.Contains(id)) return;
        if (!IsCooledDown(id, cooldown)) return;
        if (_lastChosen == id) return;
        list.Add((id, weight));
    }

    // ── Pesos ─────────────────────────────────────────────────────────────────

    private int ComputeTongueBiteWeight()
    {
        int w = tongueBiteWeight;
        if (_player != null && Mathf.Abs(_player.position.x) < tongueDistanceCentreThreshold)
            w += 2;
        return w;
    }

    private int ComputeTongueSweepWeight()
    {
        int w = tongueSweepWeight;
        // Bonus si el jugador lleva quieto un rato
        if (_playerStillTimer >= mouthStillThreshold) w += 1;
        return w;
    }

    private int ComputeTentacleWeight()
    {
        int w = tentacleWeight;
        if (_player != null && Mathf.Abs(_player.position.x) > tongueDistanceCentreThreshold)
            w += 1;
        return w;
    }

    private int ComputeMouthWeight()
    {
        int w = mouthWeight;
        if (_playerStillTimer >= mouthStillThreshold) w += 1;
        return w;
    }

    private int ComputeMinionWeight()
    {
        int w = minionWeight;
        if (_minionSpawner != null && _minionSpawner.HasLivingMinions()) w -= 2;
        return Mathf.Max(0, w);
    }

    // ── Launch ────────────────────────────────────────────────────────────────

    private void Launch(AttackId id)
    {
        _lastUsed[id] = Time.time;
        _activeAttacks.Add(id);
        _eyeTracker?.TelegraphAttack(id);

        switch (id)
        {
            case AttackId.TongueBite:
                _tongue?.ExecuteBite(() => OnAttackComplete(AttackId.TongueBite));
                break;
            case AttackId.TongueSweep:
                _tongue?.ExecuteSweep(() => OnAttackComplete(AttackId.TongueSweep));
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

    public void OnAttackComplete(AttackId id) => _activeAttacks.Remove(id);

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

    public void StopAll()
    {
        _activeAttacks.Clear();
    }

    public bool IsAttackActive(AttackId id) => _activeAttacks.Contains(id);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireCube(transform.position,
            new Vector3(tongueDistanceCentreThreshold * 2f, 12f, 0f));
    }
}
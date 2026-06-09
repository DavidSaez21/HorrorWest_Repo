using System.Collections.Generic;
using UnityEngine;

public class BossActionQueue : MonoBehaviour
{
    [Header("Pressure Budget")]
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
    [SerializeField] private float globalCooldownDuration = 1.5f;

    [Header("Context Thresholds")]
    [SerializeField] private float tongueDistanceCentreThreshold = 1.5f;
    [SerializeField] private float mouthStillThreshold = 2f;
    [SerializeField] private float playerStillSpeed = 0.5f;

    public enum AttackId { TongueBite, TongueSweep, Tentacle, GroundMouth, Minion }

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
    private int _lastChosenCount = 0;
    private float _globalCooldownEnd = 0f;

    private Transform _player;
    private Vector3 _lastPlayerPos;
    private float _playerStillTimer = 0f;

    private TongueAttack _tongue;
    private TentacleAttack _tentacleLeft;
    private TentacleAttack _tentacleRight;
    private GroundMouthAttack _groundMouth;
    private MinionSpawner _minionSpawner;
    private BossEyeTracker _eyeTracker;

    // Contador de tentáculos activos para sincronizar el callback
    private int _tentaclesActive = 0;

    public void Initialise(
        Transform player,
        TongueAttack tongue,
        TentacleAttack tentacleLeft,
        TentacleAttack tentacleRight,
        GroundMouthAttack groundMouth,
        MinionSpawner minionSpawner,
        BossEyeTracker eyeTracker)
    {
        _player = player;
        _tongue = tongue;
        _tentacleLeft = tentacleLeft;
        _tentacleRight = tentacleRight;
        _groundMouth = groundMouth;
        _minionSpawner = minionSpawner;
        _eyeTracker = eyeTracker;
        _lastPlayerPos = player != null ? player.position : Vector3.zero;
    }

    public void Tick()
    {
        if (_player == null) return;
        UpdateContextTracking();
        if (Time.time < _globalCooldownEnd) return;

        int activePressure = 0;
        foreach (var id in _activeAttacks)
            if (id != AttackId.GroundMouth) activePressure++;

        TryLaunchBackground();

        if (activePressure < maxSimultaneousAttacks)
            TryLaunchForeground();
    }

    private void UpdateContextTracking()
    {
        if (_player == null) return;
        float speed = Vector3.Distance(_player.position, _lastPlayerPos) / Time.deltaTime;
        _lastPlayerPos = _player.position;
        _playerStillTimer = speed < playerStillSpeed
            ? _playerStillTimer + Time.deltaTime
            : 0f;
    }

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

        if (_lastChosen == chosen)
        {
            _lastChosenCount++;
            if (_lastChosenCount >= 2) { _lastChosen = null; _lastChosenCount = 0; }
        }
        else
        {
            _lastChosen = chosen;
            _lastChosenCount = 1;
        }

        _globalCooldownEnd = Time.time + globalCooldownDuration;
    }

    private void TryLaunchBackground()
    {
        if (_activeAttacks.Contains(AttackId.GroundMouth)) return;
        if (!IsCooledDown(AttackId.GroundMouth, mouthCooldown)) return;
        if (ComputeMouthWeight() <= 0) return;
        Launch(AttackId.GroundMouth);
    }

    private void TryAddCandidate(List<(AttackId, int)> list, AttackId id, int weight, float cooldown)
    {
        if (weight <= 0) return;
        if (_activeAttacks.Contains(id)) return;
        if (!IsCooledDown(id, cooldown)) return;
        if (_lastChosen == id && _lastChosenCount >= 2) return;
        list.Add((id, weight));
    }

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

    private void Launch(AttackId id)
    {
        _lastUsed[id] = Time.time;
        _eyeTracker?.TelegraphAttack(id);

        switch (id)
        {
            case AttackId.TongueBite:
                if (_tongue == null) return;
                _activeAttacks.Add(id);
                _tongue.ExecuteBite(() => OnAttackComplete(AttackId.TongueBite));
                break;

            case AttackId.TongueSweep:
                if (_tongue == null) return;
                _activeAttacks.Add(id);
                _tongue.ExecuteSweep(() => OnAttackComplete(AttackId.TongueSweep));
                break;

            case AttackId.Tentacle:
                if (_tentacleLeft == null && _tentacleRight == null) return;
                _activeAttacks.Add(id);
                _tentaclesActive = 0;
                if (_tentacleLeft != null) _tentaclesActive++;
                if (_tentacleRight != null) _tentaclesActive++;
                if (_tentacleLeft != null) _tentacleLeft.Execute(OnTentacleComplete);
                if (_tentacleRight != null) _tentacleRight.Execute(OnTentacleComplete);
                break;

            case AttackId.GroundMouth:
                if (_groundMouth == null) return;
                _activeAttacks.Add(id);
                _groundMouth.Execute(() => OnAttackComplete(AttackId.GroundMouth));
                break;

            case AttackId.Minion:
                if (_minionSpawner == null) return;
                _activeAttacks.Add(id);
                _minionSpawner.Execute(() => OnAttackComplete(AttackId.Minion));
                break;
        }
    }

    private void OnTentacleComplete()
    {
        _tentaclesActive--;
        if (_tentaclesActive <= 0)
            OnAttackComplete(AttackId.Tentacle);
    }

    public void OnAttackComplete(AttackId id)
    {
        _activeAttacks.Remove(id);
        Debug.Log($"OnAttackComplete: {id} — activos: {string.Join(", ", _activeAttacks)}");
    }

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

    public void StopAll() => _activeAttacks.Clear();
    public bool IsAttackActive(AttackId id) => _activeAttacks.Contains(id);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireCube(transform.position,
            new Vector3(tongueDistanceCentreThreshold * 2f, 12f, 0f));
    }
}
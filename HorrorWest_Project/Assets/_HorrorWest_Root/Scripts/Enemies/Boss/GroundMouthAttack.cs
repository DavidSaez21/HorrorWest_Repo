using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ground mouth trap attack for the Church Boss.
///
/// Spawns 1-N mouth instances in semi-random positions in the aisle.
/// Each mouth runs through 3 animation phases independently:
///   Phase 1 — CRACK   : Closed crack appears on the floor (telegraph, no damage).
///   Phase 2 — OPEN    : Mouth fully open, hitbox active (tick damage while standing inside).
///   Phase 3 — BITE    : Mouth snaps shut, massive damage if player is caught inside.
///
/// Setup:
///   • mouthPrefab — A prefab that has a GroundMouthInstance component (see below).
///   • spawnArea   — A BoxCollider2D (trigger=false) that defines the valid spawn rectangle.
///   • minSpacing  — Minimum distance between two mouths (avoids total overlap).
/// </summary>
public class GroundMouthAttack : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private GameObject mouthPrefab;
    [Tooltip("BoxCollider2D defining the valid spawn area in the aisle.")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("Spawn Config")]
    [SerializeField] private int maxMouthsPerActivation = 2;
    [Tooltip("Minimum distance between two mouth spawn points.")]
    [SerializeField] private float minSpacing = 1.2f;
    [Tooltip("Max attempts to find a valid (non-overlapping) spawn point.")]
    [SerializeField] private int maxPlacementAttempts = 20;

    // Set by ChurchBoss from ChurchBossData
    [HideInInspector] public float crackDuration  = 0.6f;
    [HideInInspector] public float openDuration   = 0.8f;
    [HideInInspector] public float biteDuration   = 0.3f;
    [HideInInspector] public float openTickDPS    = 10f;
    [HideInInspector] public float biteDamage     = 30f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Action _onComplete;
    private Coroutine _attackCoroutine;
    private readonly List<Vector3> _usedPositions = new(); // avoids same-tile repeats

    // ── Public API ────────────────────────────────────────────────────────────

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(SpawnMouthsRoutine());
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator SpawnMouthsRoutine()
    {
        if (mouthPrefab == null || spawnArea == null)
        {
            Debug.LogWarning("[GroundMouthAttack] Missing mouthPrefab or spawnArea.");
            _onComplete?.Invoke();
            yield break;
        }

        List<Vector3> spawnPoints = GenerateSpawnPoints(maxMouthsPerActivation);
        _usedPositions.AddRange(spawnPoints);

        // Keep used positions list from growing forever
        if (_usedPositions.Count > 30) _usedPositions.RemoveRange(0, 10);

        // Launch all mouths simultaneously (each runs its own coroutine)
        int completed = 0;
        foreach (Vector3 pos in spawnPoints)
        {
            GameObject go = Instantiate(mouthPrefab, pos, Quaternion.identity);
            if (go.TryGetComponent(out GroundMouthInstance instance))
            {
                instance.Activate(crackDuration, openDuration, biteDuration, openTickDPS, biteDamage, () =>
                {
                    completed++;
                    if (completed >= spawnPoints.Count)
                    {
                        _onComplete?.Invoke();
                        _attackCoroutine = null;
                    }
                });
            }
            else
            {
                // Prefab missing GroundMouthInstance — destroy and count as done
                Destroy(go);
                completed++;
            }
        }

        if (spawnPoints.Count == 0)
        {
            _onComplete?.Invoke();
            _attackCoroutine = null;
        }
    }

    // ── Spawn point generation ────────────────────────────────────────────────

    private List<Vector3> GenerateSpawnPoints(int count)
    {
        Bounds bounds = spawnArea.bounds;
        var points = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            Vector3 candidate = Vector3.zero;
            bool found = false;

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
                float y = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);
                candidate = new Vector3(x, y, 0f);

                if (IsFarEnough(candidate, points) && IsFarEnough(candidate, _usedPositions))
                {
                    found = true;
                    break;
                }
            }

            if (found) points.Add(candidate);
        }

        return points;
    }

    private bool IsFarEnough(Vector3 candidate, List<Vector3> existing)
    {
        foreach (Vector3 p in existing)
            if (Vector3.Distance(candidate, p) < minSpacing) return false;
        return true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// GroundMouthInstance — lives on the mouthPrefab
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Controls a single mouth instance through its 3-phase lifecycle.
/// Attach to the mouth prefab alongside its Collider2D (trigger) and Animator.
///
/// Animator integration (optional but recommended):
///   Call SetTrigger("Phase_Crack"), SetTrigger("Phase_Open"), SetTrigger("Phase_Bite")
///   to drive art transitions — the coroutine timings match the animation durations.
/// </summary>
public class GroundMouthInstance : MonoBehaviour
{
    [SerializeField] private Collider2D mouthCollider; // trigger, enabled only in OPEN phase
    [SerializeField] private Animator animator;

    // Animator trigger names — match these in your Animator Controller
    private static readonly int Anim_Crack = Animator.StringToHash("Phase_Crack");
    private static readonly int Anim_Open  = Animator.StringToHash("Phase_Open");
    private static readonly int Anim_Bite  = Animator.StringToHash("Phase_Bite");

    private float _openTickDPS;
    private float _biteDamage;
    private bool _isOpen;
    private bool _isBiting;
    private HashSet<PlayerHealth> _playersInside = new();

    public void Activate(
        float crackDuration,
        float openDuration,
        float biteDuration,
        float openTickDPS,
        float biteDamage,
        Action onComplete)
    {
        _openTickDPS = openTickDPS;
        _biteDamage  = biteDamage;
        if (mouthCollider != null) mouthCollider.enabled = false;
        StartCoroutine(LifecycleRoutine(crackDuration, openDuration, biteDuration, onComplete));
    }

    private IEnumerator LifecycleRoutine(
        float crackDuration,
        float openDuration,
        float biteDuration,
        Action onComplete)
    {
        // ── Phase 1: CRACK (telegraph) ─────────────────────────────────────────
        animator?.SetTrigger(Anim_Crack);
        yield return new WaitForSeconds(crackDuration);

        // ── Phase 2: OPEN (tick damage) ───────────────────────────────────────
        _isOpen = true;
        if (mouthCollider != null) mouthCollider.enabled = true;
        animator?.SetTrigger(Anim_Open);
        yield return new WaitForSeconds(openDuration);

        // ── Phase 3: BITE (burst damage) ──────────────────────────────────────
        _isOpen   = false;
        _isBiting = true;
        if (mouthCollider != null) mouthCollider.enabled = false; // hitbox off — bite is instant
        animator?.SetTrigger(Anim_Bite);

        // Damage players still standing on the mouth at bite moment
        foreach (var ph in _playersInside)
        {
            if (ph != null) ph.TakeDamage(_biteDamage);
        }
        _playersInside.Clear();

        yield return new WaitForSeconds(biteDuration);

        onComplete?.Invoke();
        Destroy(gameObject);
    }

    // ── Tick damage while player stands inside the open mouth ─────────────────

    private void Update()
    {
        if (!_isOpen || _playersInside.Count == 0) return;
        foreach (var ph in _playersInside)
        {
            if (ph != null) ph.TakeDamage(_openTickDPS * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isOpen) return;
        if (other.TryGetComponent(out PlayerHealth ph))
            _playersInside.Add(ph);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerHealth ph))
            _playersInside.Remove(ph);
    }
}

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tentacle sweep attack for the Church Boss.
///
/// Sequence per side:
///   1. EMERGE  — tentacle tip becomes visible at the flank edge (telegraph window).
///   2. SWEEP   — tentacle moves horizontally across the aisle. Hitbox active.
///   3. RETRACT — tentacle pulls back out of frame.
///
/// The attack can run on the left, right, or both flanks simultaneously.
/// Which side(s) are used is chosen randomly, weighted toward single-side at low difficulty.
///
/// Setup in Inspector:
///   • leftTentacleRoot  — parent Transform that starts off-screen to the left.
///   • rightTentacleRoot — parent Transform that starts off-screen to the right.
///   • Each root has a child Collider2D (trigger) that is enabled during the sweep phase.
/// </summary>
public class TentacleAttack : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Transform leftTentacleRoot;
    [SerializeField] private Transform rightTentacleRoot;
    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Collider2D rightHitbox;

    [Header("Positions (X axis, world space)")]
    [Tooltip("X position the tentacle starts from (off-screen left edge).")]
    [SerializeField] private float leftHiddenX = -12f;
    [Tooltip("X position the left tentacle tip appears during telegraph.")]
    [SerializeField] private float leftTelegraphX = -5f;
    [Tooltip("X position the left tentacle reaches at full sweep.")]
    [SerializeField] private float leftSweepTargetX = 2f;

    [SerializeField] private float rightHiddenX = 12f;
    [SerializeField] private float rightTelegraphX = 5f;
    [SerializeField] private float rightSweepTargetX = -2f;

    [Header("Dual-side probability")]
    [Tooltip("0 = always single side. 1 = always both sides simultaneously.")]
    [Range(0f, 1f)]
    [SerializeField] private float bothSidesChance = 0.3f;

    [Header("Game Feel")]
    [SerializeField] private float hitStopFrames = 3f;
    [SerializeField] private float screenShakeMagnitude = 0.15f;
    [SerializeField] private float screenShakeDuration = 0.2f;
    [SerializeField] private GameObject impactParticlePrefab;

    // Set by ChurchBoss from ChurchBossData
    [HideInInspector] public float telegraphDuration = 0.4f;
    [HideInInspector] public float sweepDuration = 0.35f;
    [HideInInspector] public float retractDuration = 0.5f;
    [HideInInspector] public float damage = 18f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Action _onComplete;
    private Coroutine _attackCoroutine;

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        SetHitboxesEnabled(false);
        HideTentacles();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(TentacleRoutine());
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator TentacleRoutine()
    {
        bool doLeft = true;
        bool doRight = UnityEngine.Random.value < bothSidesChance;
        if (!doRight && UnityEngine.Random.value < 0.5f)
        {
            // Flip: only right
            doLeft = false;
            doRight = true;
        }

        SetHitboxesEnabled(false);

        // ── Phase 1: EMERGE (telegraph) ────────────────────────────────────────
        if (doLeft) MoveTentacleX(leftTentacleRoot, leftTelegraphX);
        if (doRight) MoveTentacleX(rightTentacleRoot, rightTelegraphX);

        // Tip-pulse visual feedback (animator trigger or simple scale ping if no animator)
        yield return new WaitForSeconds(telegraphDuration);

        // ── Phase 2: SWEEP (hitbox active) ────────────────────────────────────
        if (doLeft) leftHitbox.enabled = true;
        if (doRight) rightHitbox.enabled = true;

        float elapsed = 0f;
        float leftStartX = doLeft ? leftTelegraphX : leftHiddenX;
        float rightStartX = doRight ? rightTelegraphX : rightHiddenX;

        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / sweepDuration);

            if (doLeft && leftTentacleRoot != null)
            {
                Vector3 pos = leftTentacleRoot.position;
                pos.x = Mathf.Lerp(leftStartX, leftSweepTargetX, t);
                leftTentacleRoot.position = pos;
            }
            if (doRight && rightTentacleRoot != null)
            {
                Vector3 pos = rightTentacleRoot.position;
                pos.x = Mathf.Lerp(rightStartX, rightSweepTargetX, t);
                rightTentacleRoot.position = pos;
            }
            yield return null;
        }

        // ── Phase 3: RETRACT ──────────────────────────────────────────────────
        SetHitboxesEnabled(false);

        elapsed = 0f;
        float leftCurrent = doLeft ? leftSweepTargetX : leftHiddenX;
        float rightCurrent = doRight ? rightSweepTargetX : rightHiddenX;

        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / retractDuration);

            if (doLeft && leftTentacleRoot != null)
            {
                Vector3 pos = leftTentacleRoot.position;
                pos.x = Mathf.Lerp(leftCurrent, leftHiddenX, t);
                leftTentacleRoot.position = pos;
            }
            if (doRight && rightTentacleRoot != null)
            {
                Vector3 pos = rightTentacleRoot.position;
                pos.x = Mathf.Lerp(rightCurrent, rightHiddenX, t);
                rightTentacleRoot.position = pos;
            }
            yield return null;
        }

        HideTentacles();
        _onComplete?.Invoke();
    }

    // ── Trigger handling (hitbox deals damage) ────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerHealth ph)) return;

        ph.TakeDamage(damage);
        StartCoroutine(HitStop());
        SpawnImpactParticles(other.transform.position, Vector2.right); // rough direction
        // CameraShaker.Instance?.Shake(screenShakeMagnitude, screenShakeDuration);
    }

    // ── Game feel helpers ─────────────────────────────────────────────────────

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopFrames / 60f);
        Time.timeScale = 1f;
    }

    private void SpawnImpactParticles(Vector3 position, Vector2 hitDirection)
    {
        if (impactParticlePrefab == null) return;
        GameObject fx = Instantiate(impactParticlePrefab, position, Quaternion.identity);
        // Rotate particles to fly away from the hit direction
        if (hitDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(-hitDirection.y, -hitDirection.x) * Mathf.Rad2Deg;
            fx.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void SetHitboxesEnabled(bool value)
    {
        if (leftHitbox != null) leftHitbox.enabled = value;
        if (rightHitbox != null) rightHitbox.enabled = value;
    }

    private void HideTentacles()
    {
        if (leftTentacleRoot != null) MoveTentacleX(leftTentacleRoot, leftHiddenX);
        if (rightTentacleRoot != null) MoveTentacleX(rightTentacleRoot, rightHiddenX);
    }

    private static void MoveTentacleX(Transform t, float x)
    {
        if (t == null) return;
        Vector3 pos = t.position;
        pos.x = x;
        t.position = pos;
    }
}
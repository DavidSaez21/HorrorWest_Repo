using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tongue lunge attack for the Church Boss.
///
/// Sequence:
///   1. WINDUP  — tongue retracts toward the boss (visual compression). Screen shake hint.
///   2. LUNGE   — tongue shoots south down the aisle with hitbox enabled.
///   3. LINGER  — brief pause at full extension (gives player last-moment dodge window).
///   4. RETRACT — tongue pulls back to resting position.
///
/// Setup:
///   • tongueRoot — Transform at the base (boss mouth). Does not move.
///   • tongueTip  — Transform that represents the tip. Moves along the Y axis.
///   • tongueHitbox — Collider2D (trigger) attached to the tongue body or tip.
///   • restingLocalY, windupLocalY, maxLungeWorldY — set in Inspector to match your art.
/// </summary>
public class TongueAttack : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Transform tongueRoot;
    [SerializeField] private Transform tongueTip;
    [SerializeField] private Collider2D tongueHitbox;

    [Header("Positions (local Y of tongueTip relative to tongueRoot)")]
    [Tooltip("Resting local Y position of the tongue tip.")]
    [SerializeField] private float restingLocalY = 0f;
    [Tooltip("Retracted local Y during windup (pulled back into the mouth).")]
    [SerializeField] private float windupLocalY = 0.5f;
    [Tooltip("Maximum world Y position the tongue tip reaches at full extension (south).")]
    [SerializeField] private float maxLungeWorldY = -6f;

    [Header("Game Feel")]
    [SerializeField] private float hitStopFrames = 4f;
    [SerializeField] private float screenShakeMagnitude = 0.25f;
    [SerializeField] private float screenShakeDuration = 0.25f;
    [SerializeField] private float preLungeShakeMagnitude = 0.08f;
    [SerializeField] private float preLungeShakeDuration = 0.15f;
    [SerializeField] private GameObject impactParticlePrefab;

    [Header("Chromatic Aberration")]
    [Tooltip("Seconds before the lunge that the chromatic aberration effect fires.")]
    [SerializeField] private float chromaticAberrationOffset = 0.2f;
    [SerializeField] private float chromaticAberrationDuration = 0.3f;

    // Set by ChurchBoss from ChurchBossData
    [HideInInspector] public float windupDuration = 0.5f;
    [HideInInspector] public float lungeDuration = 0.25f;
    [HideInInspector] public float lingerDuration = 0.15f;
    [HideInInspector] public float retractDuration = 0.4f;
    [HideInInspector] public float damage = 25f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Action _onComplete;
    private Coroutine _attackCoroutine;
    private bool _hitPlayerThisSwing; // prevent double-damage per lunge

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (tongueHitbox != null) tongueHitbox.enabled = false;
        SnapTipToLocalY(restingLocalY);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(TongueRoutine());
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator TongueRoutine()
    {
        _hitPlayerThisSwing = false;
        if (tongueHitbox != null) tongueHitbox.enabled = false;

        // ── Phase 1: WINDUP ────────────────────────────────────────────────────
        // Small anticipatory shake
        // CameraShaker.Instance?.Shake(preLungeShakeMagnitude, preLungeShakeDuration);

        float elapsed = 0f;
        float startY = GetTipLocalY();
        while (elapsed < windupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / windupDuration);
            SnapTipToLocalY(Mathf.Lerp(startY, windupLocalY, t));
            yield return null;
        }

        // Chromatic aberration fires just before the lunge
        StartCoroutine(ChromaticAberrationRoutine());
        yield return new WaitForSeconds(chromaticAberrationOffset);

        // ── Phase 2: LUNGE ─────────────────────────────────────────────────────
        if (tongueHitbox != null) tongueHitbox.enabled = true;

        elapsed = 0f;
        float lungeStartWorldY = tongueRoot != null
            ? tongueRoot.position.y + windupLocalY
            : transform.position.y + windupLocalY;

        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lungeDuration);
            float worldY = Mathf.Lerp(lungeStartWorldY, maxLungeWorldY, t);
            SetTipWorldY(worldY);
            yield return null;
        }

        // ── Phase 3: LINGER ────────────────────────────────────────────────────
        yield return new WaitForSeconds(lingerDuration);

        // ── Phase 4: RETRACT ───────────────────────────────────────────────────
        if (tongueHitbox != null) tongueHitbox.enabled = false;

        elapsed = 0f;
        float retractStartLocalY = GetTipLocalY();
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / retractDuration);
            SnapTipToLocalY(Mathf.Lerp(retractStartLocalY, restingLocalY, t));
            yield return null;
        }

        SnapTipToLocalY(restingLocalY);
        _attackCoroutine = null;
        _onComplete?.Invoke();
    }

    // ── Chromatic aberration ──────────────────────────────────────────────────

    private IEnumerator ChromaticAberrationRoutine()
    {
        // Hook into your post-processing stack here.
        // If using URP with a Volume, find the ChromaticAberration override and animate it.
        // Placeholder: just logs so the hook point is obvious.
        // Example with URP Volume:
        //   var ca = postProcessVolume.profile.TryGet<ChromaticAberration>(out var effect);
        //   effect.intensity.value = 1f;
        //   yield return new WaitForSeconds(chromaticAberrationDuration);
        //   effect.intensity.value = 0f;
        yield return new WaitForSeconds(chromaticAberrationDuration);
    }

    // ── Trigger: damage on contact ────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitPlayerThisSwing) return;
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerHealth ph)) return;

        _hitPlayerThisSwing = true; // one damage event per lunge
        ph.TakeDamage(damage);
        StartCoroutine(HitStop());
        // CameraShaker.Instance?.Shake(screenShakeMagnitude, screenShakeDuration);
        SpawnImpactParticles(other.transform.position, Vector2.up);
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
        if (hitDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(-hitDirection.y, -hitDirection.x) * Mathf.Rad2Deg;
            fx.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // ── Position helpers ──────────────────────────────────────────────────────

    private void SnapTipToLocalY(float localY)
    {
        if (tongueTip == null) return;
        Vector3 pos = tongueTip.localPosition;
        pos.y = localY;
        tongueTip.localPosition = pos;
    }

    private float GetTipLocalY()
        => tongueTip != null ? tongueTip.localPosition.y : 0f;

    private void SetTipWorldY(float worldY)
    {
        if (tongueTip == null) return;
        Vector3 pos = tongueTip.position;
        pos.y = worldY;
        tongueTip.position = pos;
    }
}
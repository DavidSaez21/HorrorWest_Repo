using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Dos ataques de lengua: Mordida y Barrido.
/// El Animator gestiona animaciones y hitbox (configurado en Animation window).
/// El script lanza los triggers y notifica al BossActionQueue cuando termina.
/// Daños y duraciones configurables desde ChurchBoss via ChurchBossData.
/// </summary>
public class TongueAttack : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Referencias")]
    [SerializeField] private Animator tongueAnimator;

    [Header("Game Feel")]
    [SerializeField] private float hitStopFrames = 4f;
    [SerializeField] private GameObject impactParticlePrefab;

    // Configurados por ChurchBoss desde ChurchBossData
    [HideInInspector] public float biteDamage = 25f;
    [HideInInspector] public float sweepDamage = 20f;
    [HideInInspector] public float biteDuration = 0.8f;
    [HideInInspector] public float sweepDuration = 1.0f;

    // ── Animator hashes ───────────────────────────────────────────────────────

    private static readonly int AnimBite = Animator.StringToHash("Bite");
    private static readonly int AnimSweep = Animator.StringToHash("Sweep");

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Action _onComplete;
    private Coroutine _attackCoroutine;
    private float _currentDamage;

    // ── Public API ────────────────────────────────────────────────────────────

    public void ExecuteBite(Action onComplete)
    {
        _currentDamage = biteDamage;
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackRoutine(AnimBite, biteDuration));
    }

    public void ExecuteSweep(Action onComplete)
    {
        _currentDamage = sweepDamage;
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(AttackRoutine(AnimSweep, sweepDuration));
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator AttackRoutine(int animTrigger, float duration)
    {
        tongueAnimator?.SetTrigger(animTrigger);
        yield return new WaitForSeconds(duration);
        _attackCoroutine = null;
        _onComplete?.Invoke();
    }

    // ── Daño al player ────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerHealth ph)) return;

        ph.TakeDamage(_currentDamage);
        StartCoroutine(HitStop());
        SpawnImpactParticles(other.transform.position);
    }

    // ── Game feel ─────────────────────────────────────────────────────────────

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopFrames / 60f);
        Time.timeScale = 1f;
    }

    private void SpawnImpactParticles(Vector3 position)
    {
        if (impactParticlePrefab == null) return;
        Instantiate(impactParticlePrefab, position, Quaternion.identity);
    }
}
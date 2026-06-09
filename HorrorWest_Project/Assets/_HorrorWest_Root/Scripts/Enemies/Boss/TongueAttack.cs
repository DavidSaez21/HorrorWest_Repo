using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tongue attack for the Church Boss.
/// Este script va en el mismo GameObject que el Animator de Tongue.
/// Las animaciones terminan mediante Animation Events — sin duraciones hardcodeadas.
/// </summary>
public class TongueAttack : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator tongueAnimator;

    [Header("Game Feel")]
    [SerializeField] private float hitStopFrames = 4f;
    [SerializeField] private GameObject impactParticlePrefab;

    // Daños configurados por ChurchBoss desde ChurchBossData
    [HideInInspector] public float biteDamage = 25f;
    [HideInInspector] public float sweepDamage = 20f;

    private static readonly int AnimBite = Animator.StringToHash("Bite");
    private static readonly int AnimSweep = Animator.StringToHash("Sweep");

    private Action _onComplete;
    private float _currentDamage;

    // ── Public API ────────────────────────────────────────────────────────────

    public void ExecuteBite(Action onComplete)
    {
        _currentDamage = biteDamage;
        _onComplete = onComplete;
        tongueAnimator?.SetTrigger(AnimBite);
    }

    public void ExecuteSweep(Action onComplete)
    {
        _currentDamage = sweepDamage;
        _onComplete = onComplete;
        tongueAnimator?.SetTrigger(AnimSweep);
    }

    // ── Animation Event — llamado desde el último frame de Bite y Sweep ───────

    public void OnAttackAnimationEnd()
    {
        _onComplete?.Invoke();
        _onComplete = null;
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
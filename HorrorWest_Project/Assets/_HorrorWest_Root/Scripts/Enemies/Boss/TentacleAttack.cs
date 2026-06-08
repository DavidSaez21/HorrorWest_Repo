using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tentacle sweep attack for the Church Boss.
///
/// Flujo:
///   1. EMERGE    — aparece en el borde, animación Idle, se mueve hacia TargetPosition.
///   2. ESPERA    — llega al target, se queda quieto waitAtTargetDuration segundos.
///   3. ATTACKING — lanza trigger "Attack" en el Animator, hitbox activa,
///                  espera a que termine la animación.
///   4. RETRACT   — último frame congelado, vuelve al punto de inicio y se oculta.
/// </summary>
public class TentacleAttack : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Tentáculo Izquierdo")]
    [SerializeField] private Transform leftTentacleRoot;
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Collider2D leftHitbox;
    [SerializeField] private Animator leftAnimator;

    [Header("Tentáculo Derecho")]
    [SerializeField] private Transform rightTentacleRoot;
    [SerializeField] private Transform rightTarget;
    [SerializeField] private Collider2D rightHitbox;
    [SerializeField] private Animator rightAnimator;

    [Header("Tiempos")]
    [Tooltip("Segundos que tarda en llegar desde el borde hasta el target.")]
    [SerializeField] private float bothSidesChance = 0.3f;

    [Header("Probabilidad doble")]
    [Range(0f, 1f)]
    [Tooltip("0 = siempre un lado. 1 = siempre los dos a la vez.")]
    [SerializeField] private float hitStopFrames = 3f;

    [Header("Game Feel")]
    [SerializeField] private GameObject impactParticlePrefab;

    // Configurados por ChurchBoss desde ChurchBossData
    [HideInInspector] public float damage = 18f;
    [HideInInspector] public float emergeDuration = 0.8f;
    [HideInInspector] public float waitAtTargetDuration = 0.5f;
    [HideInInspector] public float attackAnimDuration = 0.6f;
    [HideInInspector] public float retractDuration = 0.5f;

    // ── Animator hashes ───────────────────────────────────────────────────────

    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Action _onComplete;
    private Coroutine _attackCoroutine;
    private Vector3 _leftStartPos;
    private Vector3 _rightStartPos;

    // ── Init ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (leftTentacleRoot != null) _leftStartPos = leftTentacleRoot.position;
        if (rightTentacleRoot != null) _rightStartPos = rightTentacleRoot.position;

        SetHitboxesEnabled(false);
        SetVisible(false, false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
        _attackCoroutine = StartCoroutine(TentacleRoutine());
    }

    // ── Coroutine principal ───────────────────────────────────────────────────

    private IEnumerator TentacleRoutine()
    {
        bool doLeft = true;
        bool doRight = UnityEngine.Random.value < bothSidesChance;
        if (!doRight && UnityEngine.Random.value < 0.5f)
        {
            doLeft = false;
            doRight = true;
        }

        SetHitboxesEnabled(false);

        // ── FASE 1: EMERGE ────────────────────────────────────────────────────
        if (doLeft && leftTentacleRoot != null)
        {
            leftTentacleRoot.position = _leftStartPos;
            leftAnimator?.Play("AC_Idle_TentaculoIZQ");
            SetVisible(true, false);
        }
        if (doRight && rightTentacleRoot != null)
        {
            rightTentacleRoot.position = _rightStartPos;
            rightAnimator?.Play("AC_Idle_TentaculoDER");
            SetVisible(false, true);
        }

        float elapsed = 0f;
        while (elapsed < emergeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / emergeDuration);

            if (doLeft && leftTentacleRoot != null && leftTarget != null)
                leftTentacleRoot.position = Vector3.Lerp(_leftStartPos, leftTarget.position, t);
            if (doRight && rightTentacleRoot != null && rightTarget != null)
                rightTentacleRoot.position = Vector3.Lerp(_rightStartPos, rightTarget.position, t);

            yield return null;
        }

        // ── FASE 2: ESPERA ────────────────────────────────────────────────────
        yield return new WaitForSeconds(waitAtTargetDuration);

        // ── FASE 3: ATTACKING ─────────────────────────────────────────────────
        if (doLeft) leftHitbox.enabled = true;
        if (doRight) rightHitbox.enabled = true;

        if (doLeft) leftAnimator?.SetTrigger(AnimAttack);
        if (doRight) rightAnimator?.SetTrigger(AnimAttack);

        yield return new WaitForSeconds(attackAnimDuration);

        // ── FASE 4: RETRACT ───────────────────────────────────────────────────
        SetHitboxesEnabled(false);

        elapsed = 0f;
        Vector3 leftCurrentPos = doLeft ? leftTentacleRoot.position : _leftStartPos;
        Vector3 rightCurrentPos = doRight ? rightTentacleRoot.position : _rightStartPos;

        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / retractDuration);

            if (doLeft && leftTentacleRoot != null)
                leftTentacleRoot.position = Vector3.Lerp(leftCurrentPos, _leftStartPos, t);
            if (doRight && rightTentacleRoot != null)
                rightTentacleRoot.position = Vector3.Lerp(rightCurrentPos, _rightStartPos, t);

            yield return null;
        }

        SetVisible(false, false);
        if (leftTentacleRoot != null) leftTentacleRoot.position = _leftStartPos;
        if (rightTentacleRoot != null) rightTentacleRoot.position = _rightStartPos;

        _attackCoroutine = null;
        _onComplete?.Invoke();
    }

    // ── Daño al player ────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerHealth ph)) return;

        ph.TakeDamage(damage);
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

    // ── Utilidades ────────────────────────────────────────────────────────────

    private void SetHitboxesEnabled(bool value)
    {
        if (leftHitbox != null) leftHitbox.enabled = value;
        if (rightHitbox != null) rightHitbox.enabled = value;
    }

    private void SetVisible(bool left, bool right)
    {
        if (leftTentacleRoot != null)
        {
            var sr = leftTentacleRoot.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = left;
        }
        if (rightTentacleRoot != null)
        {
            var sr = rightTentacleRoot.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = right;
        }
    }
}
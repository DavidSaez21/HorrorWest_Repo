using System.Collections;
using UnityEngine;

/// <summary>
/// Rotates the boss's eye transforms to track the player every frame.
/// When an attack is telegraphed, briefly redirects the eyes toward the
/// attack's origin zone as a subtle (but readable) visual tell.
///
/// Setup:
///   Assign leftEye and rightEye in the Inspector.
///   Both Transforms should be children of the boss sprite.
///   The "forward" direction of each eye is assumed to be its local Up (+Y).
/// </summary>
public class BossEyeTracker : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Eye Transforms")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;

    [Header("Tracking")]
    [Tooltip("How quickly the eyes snap toward their target direction.")]
    [SerializeField] private float trackingSpeed = 8f;

    [Header("Telegraph")]
    [Tooltip("Duration the eyes hold on the attack zone before resuming player tracking.")]
    [SerializeField] private float telegraphHoldDuration = 0.5f;

    [Header("Zone World Positions")]
    [Tooltip("World position hint for the Tongue attack zone (front-centre of the altar).")]
    [SerializeField] private Transform tongueZone;
    [Tooltip("World position hint for the left Tentacle zone.")]
    [SerializeField] private Transform tentacleLeftZone;
    [Tooltip("World position hint for the right Tentacle zone.")]
    [SerializeField] private Transform tentacleRightZone;
    [Tooltip("World position hint for the south entrance (Minion spawn).")]
    [SerializeField] private Transform minionZone;
    [Tooltip("World position hint for the centre of the aisle (Ground Mouth zone).")]
    [SerializeField] private Transform mouthZone;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Transform _player;
    private Transform _leftTarget;
    private Transform _rightTarget;
    private bool _isTelegraphing = false;
    private Coroutine _telegraphCoroutine;

    // ── Init ──────────────────────────────────────────────────────────────────

    public void Initialise(Transform player)
    {
        _player = player;
        ResumeTracking();
    }

    // ── Per-frame tracking ────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (leftEye != null && _leftTarget != null)
            RotateEyeToward(leftEye, _leftTarget.position);

        if (rightEye != null && _rightTarget != null)
            RotateEyeToward(rightEye, _rightTarget.position);
    }

    private void RotateEyeToward(Transform eye, Vector3 worldTarget)
    {
        Vector2 dir = (worldTarget - eye.position).normalized;
        if (dir.sqrMagnitude < 0.001f) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = eye.rotation.eulerAngles.z;

        // Shortest-path interpolation
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);
        float newAngle = currentAngle + delta * Mathf.Clamp01(trackingSpeed * Time.deltaTime);

        eye.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    // ── Telegraph ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Redirects the eyes toward the attack zone for <see cref="telegraphHoldDuration"/> seconds,
    /// then resumes player tracking. Called by BossActionQueue before launching an attack.
    /// </summary>
    public void TelegraphAttack(BossActionQueue.AttackId attackId)
    {
        if (_telegraphCoroutine != null)
            StopCoroutine(_telegraphCoroutine);

        _telegraphCoroutine = StartCoroutine(TelegraphRoutine(attackId));
    }

    private IEnumerator TelegraphRoutine(BossActionQueue.AttackId attackId)
    {
        _isTelegraphing = true;
        SetEyeTargets(attackId);
        yield return new WaitForSeconds(telegraphHoldDuration);
        _isTelegraphing = false;
        ResumeTracking();
        _telegraphCoroutine = null;
    }

    private void SetEyeTargets(BossActionQueue.AttackId attackId)
    {
        switch (attackId)
        {
            case BossActionQueue.AttackId.Tongue:
                // Both eyes look straight ahead (toward the player / south)
                SetBothEyes(GetPlayerProxyTransform());
                break;

            case BossActionQueue.AttackId.Tentacle:
                // Each eye looks toward its own flank
                _leftTarget  = tentacleLeftZone  != null ? tentacleLeftZone  : GetPlayerProxyTransform();
                _rightTarget = tentacleRightZone != null ? tentacleRightZone : GetPlayerProxyTransform();
                break;

            case BossActionQueue.AttackId.GroundMouth:
                // Eyes look slightly downward (toward the aisle floor)
                SetBothEyes(mouthZone != null ? mouthZone : GetPlayerProxyTransform());
                break;

            case BossActionQueue.AttackId.Minion:
                // Eyes look toward the south entrance
                SetBothEyes(minionZone != null ? minionZone : GetPlayerProxyTransform());
                break;
        }
    }

    private void SetBothEyes(Transform target)
    {
        _leftTarget = target;
        _rightTarget = target;
    }

    private void ResumeTracking()
    {
        if (_player == null) return;
        SetBothEyes(_player);
    }

    // ── Utility: proxy transform that matches the player's position ────────────

    // We use the player Transform directly when available.
    // This method exists so SetEyeTargets always has a non-null fallback.
    private Transform GetPlayerProxyTransform() => _player;
}

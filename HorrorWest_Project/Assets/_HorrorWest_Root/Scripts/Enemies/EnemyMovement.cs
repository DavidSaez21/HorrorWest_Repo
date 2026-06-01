using UnityEngine;

/// <summary>
/// Enemy movement using a two-layer approach:
///
///   Layer 1 — Flow Field (global navigation)
///     Reads the pre-computed FlowFieldManager gradient to know which direction
///     leads to the player even when walls are in the way. Enemies always know
///     where you are and can navigate around any static obstacle.
///
///   Layer 2 — Context Steering (local avoidance)
///     Samples directions around the enemy and masks out any that are too close
///     to an immediate obstacle or another enemy. Prevents clipping and stacking.
///
/// The two layers are blended: flow field gives the global goal, context steering
/// picks the best local direction that aligns with it.
/// </summary>
public class EnemyMovement : EnemyBase
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Context Steering")]
    [Tooltip("Directions sampled each frame. 16 works well with flow field — " +
             "flow field handles global nav so local steering can be lighter.")]
    [Range(8, 32)]
    [SerializeField] private int _contextSlots = 16;

    [Tooltip("Ray length for local obstacle probes. Keep around sprite width.")]
    [Range(0.3f, 2f)]
    [SerializeField] private float _probeLength = 0.9f;

    [Tooltip("Slots with danger above this are masked. Lower = reacts sooner to obstacles.")]
    [Range(0f, 1f)]
    [SerializeField] private float _dangerThreshold = 0.2f;

    [Tooltip("How fast the heading smooths toward the chosen direction. " +
             "8-12 feels physical without being sluggish.")]
    [Range(3f, 20f)]
    [SerializeField] private float _steerSmoothing = 10f;

    [SerializeField] private LayerMask _obstacleLayerMask;

    // ── Context steering arrays ───────────────────────────────────────────────

    private Vector2[] _slotDirections;
    private float[] _interest;
    private float[] _danger;
    private Vector2 _currentHeading;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        AllocateContextArrays();
    }

    private void AllocateContextArrays()
    {
        _slotDirections = new Vector2[_contextSlots];
        _interest = new float[_contextSlots];
        _danger = new float[_contextSlots];

        float step = 360f / _contextSlots;
        for (int i = 0; i < _contextSlots; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            _slotDirections[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }

    // ── EnemyBase override ────────────────────────────────────────────────────

    protected override void UpdateSteering()
    {
        float dist = DistanceToPlayer();

        // Stop inside attack range — crowd the edge, don't push through.
        if (dist <= data.attackRange)
        {
            DesiredVelocity = Vector2.zero;
            return;
        }

        // ── 1. Get global direction from flow field ────────────────────────
        // This direction already knows how to navigate around walls.
        Vector2 flowDir = FlowFieldManager.Instance != null
            ? FlowFieldManager.Instance.GetDirection(transform.position)
            : DirectionToPlayer(); // fallback if manager not in scene

        // ── 2. Apply context steering — pick the best local slot ──────────
        Vector2 steerDir = ApplyContextSteering(flowDir);

        // ── 3. Smooth heading ──────────────────────────────────────────────
        _currentHeading = Vector2.Lerp(
            _currentHeading,
            steerDir,
            _steerSmoothing * Time.fixedDeltaTime
        ).normalized;

        FaceDirection(_currentHeading);
        DesiredVelocity = _currentHeading * data.moveSpeed;
    }

    // ── Context steering ──────────────────────────────────────────────────────

    private Vector2 ApplyContextSteering(Vector2 desiredDir)
    {
        // Interest: alignment with the flow field direction
        for (int i = 0; i < _contextSlots; i++)
            _interest[i] = Mathf.Max(0f, Vector2.Dot(_slotDirections[i], desiredDir));

        // Danger: proximity to obstacles in each slot direction
        for (int i = 0; i < _contextSlots; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, _slotDirections[i], _probeLength, _obstacleLayerMask);

            _danger[i] = hit.collider != null
                ? 1f - Mathf.Clamp01(hit.distance / _probeLength)
                : 0f;
        }

        // Pick best unmasked slot
        Vector2 best = desiredDir;
        float bestScore = -1f;
        bool anyValid = false;

        for (int i = 0; i < _contextSlots; i++)
        {
            if (_danger[i] > _dangerThreshold) continue;

            if (_interest[i] > bestScore)
            {
                bestScore = _interest[i];
                best = _slotDirections[i];
                anyValid = true;
            }
        }

        // Fallback: completely surrounded — pick safest slot (slide along wall)
        if (!anyValid)
        {
            float lowestDanger = float.MaxValue;
            for (int i = 0; i < _contextSlots; i++)
            {
                if (_danger[i] < lowestDanger)
                {
                    lowestDanger = _danger[i];
                    best = _slotDirections[i];
                }
            }
        }

        return best;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (!Application.isPlaying || _slotDirections == null) return;

        // Flow field direction (cyan)
        if (FlowFieldManager.Instance != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position,
                (Vector3)FlowFieldManager.Instance.GetDirection(transform.position) * 1.2f);
        }

        // Context slots
        for (int i = 0; i < _contextSlots; i++)
        {
            bool masked = _danger[i] > _dangerThreshold;
            Gizmos.color = masked
                ? new Color(1f, 0.1f, 0.1f, 0.7f)
                : new Color(1f - _interest[i], _interest[i], 0f, 0.4f);
            Gizmos.DrawRay(transform.position,
                _slotDirections[i] * _probeLength * (masked ? 0.5f : Mathf.Max(_interest[i], 0.15f)));
        }

        // Final heading (white)
        if (_currentHeading != Vector2.zero)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, _currentHeading * 1.5f);
        }
    }
}
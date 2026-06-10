using UnityEngine;

/// <summary>
/// Ranged enemy that feels like a human shooter:
/// - Rotates toward the player with limited angular speed (no aimbot snap)
/// - Wanders to random nearby waypoints instead of orbiting
/// - Stops briefly to shoot, then picks a new waypoint
/// - Uses flow field to navigate around walls
/// - Never shoots through walls (line of sight check)
/// </summary>
public class EnemyShooter : EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _muzzle;

    [Header("Ranges")]
    [Range(2f, 8f)][SerializeField] private float _minRange = 3f;
    [Range(4f, 14f)][SerializeField] private float _maxRange = 6f;

    [Header("Rotation")]
    [Tooltip("Max degrees per second the enemy can rotate toward the player. " +
             "Low values = feels human, high values = feels like a turret.")]
    [Range(30f, 360f)]
    [SerializeField] private float _rotationSpeed = 90f;

    [Header("Shooting Behaviour")]
    [Tooltip("Enemy only shoots when aimed within this angle of the player.")]
    [Range(5f, 40f)]
    [SerializeField] private float _shootAngleTolerance = 15f;

    [Tooltip("After shooting, waits this long before moving again.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _postShotPause = 0.4f;

    [Header("Wandering")]
    [Tooltip("How far from current position the enemy picks its next waypoint.")]
    [Range(1f, 5f)]
    [SerializeField] private float _wanderRadius = 3f;

    [Tooltip("How close the enemy needs to get to a waypoint before picking a new one.")]
    [Range(0.3f, 1.5f)]
    [SerializeField] private float _waypointReachedDist = 0.6f;

    [Tooltip("Wander speed as fraction of moveSpeed.")]
    [Range(0.3f, 1f)]
    [SerializeField] private float _wanderSpeedFraction = 0.6f;

    [Header("Context Steering")]
    [Range(8, 32)][SerializeField] private int _contextSlots = 16;
    [Range(0.3f, 2f)][SerializeField] private float _probeLength = 0.7f;
    [Range(0f, 1f)][SerializeField] private float _dangerThreshold = 0.25f;
    [Range(3f, 20f)][SerializeField] private float _steerSmoothing = 10f;
    [SerializeField] private LayerMask _obstacleLayerMask;

    // ── Internal state ────────────────────────────────────────────────────────

    private enum MoveState { Approaching, Wandering, PausedToShoot }
    private MoveState _state = MoveState.Approaching;

    // Current facing angle in degrees (world space, 0 = right)
    private float _facingAngle;

    // Wander waypoint
    private Vector2 _waypoint;
    private bool _hasWaypoint;

    // Post-shot pause timer
    private float _pauseTimer;

    // Context steering
    private Vector2[] _slotDirections;
    private float[] _interest;
    private float[] _danger;
    private Vector2 _currentHeading;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        BuildSlots();
    }

    protected override void Start()
    {
        base.Start();
        // Initialise facing to a random direction so enemies don't all start identical
        _facingAngle = Random.Range(0f, 360f);
    }

    private void BuildSlots()
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

    // ── Rotation with inertia ─────────────────────────────────────────────────

    /// <summary>
    /// Rotates the sprite toward targetDir at a limited angular speed.
    /// Feels like a person turning their body, not a turret snapping.
    /// </summary>
    private void RotateToward(Vector2 targetDir)
    {
        if (targetDir == Vector2.zero) return;

        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        _facingAngle = Mathf.MoveTowardsAngle(
            _facingAngle, targetAngle, _rotationSpeed * Time.fixedDeltaTime);

        // Apply rotation (sprite up = forward, hence -90)
        transform.rotation = Quaternion.Euler(0f, 0f, _facingAngle - 90f);
    }

    /// <summary>Returns the angle between current facing and the player in degrees.</summary>
    private float AngleToPlayer()
    {
        Vector2 toPlayer = DirectionToPlayer();
        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        return Mathf.Abs(Mathf.DeltaAngle(_facingAngle, targetAngle));
    }

    // ── Shooting ──────────────────────────────────────────────────────────────

    // Override Update — shoot only when aimed correctly and has LOS
    protected override void Update()
    {
        if (isDead || player == null) return;

        bool inRange = DistanceToPlayer() <= _maxRange;
        bool aimed = AngleToPlayer() <= _shootAngleTolerance;
        bool hasLos = HasLineOfSight();

        if (inRange && aimed && hasLos && CanAttack())
        {
            PerformAttack();
            ResetAttackCooldown();
        }
    }

    protected override void PerformAttack()
    {
        if (_bulletPrefab == null) return;

        Transform origin = _muzzle != null ? _muzzle : transform;

        Vector2 shootDir = DirectionToPlayer();
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg - 90f;

        Instantiate(_bulletPrefab, origin.position, Quaternion.Euler(0f, 0f, angle));

        _pauseTimer = _postShotPause;
        _state = MoveState.PausedToShoot;
    }
    // ── Movement ──────────────────────────────────────────────────────────────

    protected override void UpdateSteering()
    {
        float dist = DistanceToPlayer();
        Vector2 toPlayer = DirectionToPlayer();

        if (toPlayer == Vector2.zero) { DesiredVelocity = Vector2.zero; return; }

        // Always rotate toward player (with inertia)
        RotateToward(toPlayer);

        // ── State logic ────────────────────────────────────────────────────

        // Post-shot pause
        if (_state == MoveState.PausedToShoot)
        {
            _pauseTimer -= Time.fixedDeltaTime;
            if (_pauseTimer <= 0f)
            {
                PickNewWaypoint();
                _state = MoveState.Wandering;
            }
            DesiredVelocity = Vector2.zero;
            return;
        }

        // Too far or no LOS → approach via flow field
        if (dist > _maxRange || !HasLineOfSight())
        {
            _state = MoveState.Approaching;
        }
        // Too close → back away
        else if (dist < _minRange)
        {
            Vector2 retreatDir = ApplyContextSteering(-toPlayer);
            _currentHeading = Vector2.Lerp(
                _currentHeading, retreatDir, _steerSmoothing * Time.fixedDeltaTime).normalized;
            DesiredVelocity = _currentHeading * data.moveSpeed * 0.8f;
            return;
        }
        // In range and has LOS → wander
        else if (_state != MoveState.Wandering)
        {
            PickNewWaypoint();
            _state = MoveState.Wandering;
        }

        // ── Move based on state ────────────────────────────────────────────
        Vector2 desiredDir;
        float speedMult;

        if (_state == MoveState.Approaching)
        {
            desiredDir = FlowFieldManager.Instance != null
                ? FlowFieldManager.Instance.GetDirection(transform.position)
                : toPlayer;
            speedMult = 1f;
        }
        else // Wandering
        {
            // Check if waypoint reached — pick a new one
            if (!_hasWaypoint ||
                Vector2.Distance(transform.position, _waypoint) < _waypointReachedDist)
                PickNewWaypoint();

            desiredDir = ((Vector2)_waypoint - (Vector2)transform.position).normalized;
            speedMult = _wanderSpeedFraction;
        }

        Vector2 steered = ApplyContextSteering(desiredDir);
        _currentHeading = Vector2.Lerp(
            _currentHeading, steered, _steerSmoothing * Time.fixedDeltaTime).normalized;

        DesiredVelocity = _currentHeading * data.moveSpeed * speedMult;
    }

    // ── Waypoint ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Picks a random nearby point that stays within the preferred engagement
    /// range band. Biased to move the enemy around rather than straight at you.
    /// </summary>
    private void PickNewWaypoint()
    {
        Vector2 toPlayer = DirectionToPlayer();
        Vector2 lateral = new Vector2(-toPlayer.y, toPlayer.x);
        float dist = DistanceToPlayer();

        // Random lateral offset + small radial correction to stay in range band
        float lateralAmt = Random.Range(-_wanderRadius, _wanderRadius);
        float radialTarget = Mathf.Clamp(dist, _minRange + 0.5f, _maxRange - 0.5f);
        float radialCorr = (radialTarget - dist) * 0.5f; // gentle pull toward band centre

        _waypoint = (Vector2)transform.position
                     + lateral * lateralAmt
                     + toPlayer * radialCorr;
        _hasWaypoint = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool HasLineOfSight()
    {
        Vector2 origin = _muzzle != null
            ? (Vector2)_muzzle.position
            : (Vector2)transform.position;
        return Physics2D.Linecast(origin, player.position, _obstacleLayerMask).collider == null;
    }

    private Vector2 ApplyContextSteering(Vector2 desiredDir)
    {
        for (int i = 0; i < _contextSlots; i++)
            _interest[i] = Mathf.Max(0f, Vector2.Dot(_slotDirections[i], desiredDir));

        for (int i = 0; i < _contextSlots; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, _slotDirections[i], _probeLength, _obstacleLayerMask);
            _danger[i] = hit.collider != null
                ? 1f - Mathf.Clamp01(hit.distance / _probeLength) : 0f;
        }

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

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _minRange);
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _maxRange);

        if (!Application.isPlaying) return;

        if (_hasWaypoint && _state == MoveState.Wandering)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _waypoint);
            Gizmos.DrawWireSphere(_waypoint, 0.2f);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f,
            $"{_state} | aim:{AngleToPlayer():F0}°");
#endif
    }
}
using UnityEngine;

public class EnemyMovement : EnemyBase
{
    [Header("Evasión de Obstáculos")]
    [SerializeField] private float _obstacleCheckCircleRadius = 0.4f;
    [SerializeField] private float _obstacleCheckDistance = 0.8f;
    [SerializeField] private float _wallScanDistance = 6f;
    [SerializeField] private LayerMask _obstacleLayerMask;
    [SerializeField] private float _rotationSpeed = 180f;

    [Header("Referencias")]
    [SerializeField] private BoxDetector2D detector;

    private RaycastHit2D[] _obstacleCollisions = new RaycastHit2D[10];
    private Vector2 _obstacleAvoidanceTargetDirection;
    private bool _isAvoiding = false;
    private Vector2 _currentMoveDir;
    private float _avoidanceMinTime = 0.3f;     // Tiempo mínimo esquivando antes de poder cancelar
    private float _avoidanceTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        if (detector == null)
            detector = GetComponent<BoxDetector2D>();
    }

    protected override void UpdateMovement(float distToPlayer)
    {
        if (detector == null || !detector.PlayerDetected || detector.Target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = ((Vector2)detector.Target.position - (Vector2)transform.position).normalized;

        // Actualiza timer de esquiva
        if (_isAvoiding)
        {
            _avoidanceTimer += Time.deltaTime;

            // Cancela la esquiva si ya pasó el tiempo mínimo Y el camino al player está libre
            if (_avoidanceTimer >= _avoidanceMinTime && IsPathClear(toPlayer))
                _isAvoiding = false;
        }

        // Solo busca nuevo obstáculo si no está esquivando
        if (!_isAvoiding)
            HandleObstacles(toPlayer);

        Vector2 targetDir = _isAvoiding ? _obstacleAvoidanceTargetDirection : toPlayer;
        _currentMoveDir = Vector2.MoveTowards(_currentMoveDir, targetDir, _rotationSpeed * Time.deltaTime * Mathf.Deg2Rad);

        FaceDirection(_currentMoveDir);

        if (distToPlayer > data.attackRange)
            rb.linearVelocity = _currentMoveDir * data.moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    // Comprueba si el camino directo al player está libre
    private bool IsPathClear(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position, _obstacleCheckCircleRadius, direction,
            _obstacleCheckDistance * 1.5f, _obstacleLayerMask
        );
        return hit.collider == null;
    }

    private void HandleObstacles(Vector2 moveDir)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(_obstacleLayerMask);
        filter.useTriggers = false;

        int hitCount = Physics2D.CircleCast(
            transform.position, _obstacleCheckCircleRadius, moveDir,
            filter, _obstacleCollisions, _obstacleCheckDistance
        );

        if (hitCount <= 0) return;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D collision = _obstacleCollisions[i];
            if (collision.collider == null) continue;
            if (collision.collider.gameObject == gameObject) continue;

            Vector2 normal = collision.normal;
            Vector2 slideLeft = new Vector2(-normal.y, normal.x);
            Vector2 slideRight = new Vector2(normal.y, -normal.x);

            float freeLeft = MeasureFreeSpace(slideLeft);
            float freeRight = MeasureFreeSpace(slideRight);

            Vector2 toPlayer = detector.Target != null
                ? ((Vector2)detector.Target.position - (Vector2)transform.position).normalized
                : moveDir;

            float scoreLeft = freeLeft + Vector2.Dot(slideLeft, toPlayer) * 2f;
            float scoreRight = freeRight + Vector2.Dot(slideRight, toPlayer) * 2f;

            _obstacleAvoidanceTargetDirection = scoreLeft > scoreRight ? slideLeft : slideRight;
            _isAvoiding = true;
            _avoidanceTimer = 0f;
            break;
        }
    }

    private float MeasureFreeSpace(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _wallScanDistance, _obstacleLayerMask);
        return hit.collider != null ? hit.distance : _wallScanDistance;
    }

    private void OnDrawGizmosSelected()
    {
        if (_currentMoveDir == Vector2.zero) return;
        Gizmos.color = _isAvoiding ? Color.red : Color.yellow;
        Vector3 castEnd = transform.position + (Vector3)(_currentMoveDir * _obstacleCheckDistance);
        Gizmos.DrawWireSphere(castEnd, _obstacleCheckCircleRadius);
        Gizmos.DrawLine(transform.position, castEnd);

        if (_isAvoiding)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, _obstacleAvoidanceTargetDirection * _wallScanDistance);
        }
    }
}
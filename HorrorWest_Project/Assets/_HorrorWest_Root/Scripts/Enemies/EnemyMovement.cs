using UnityEngine;

public class EnemyMovement : EnemyBase
{
    [Header("Evasión de Obstáculos")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _obstacleCheckCircleRadius = 0.4f;
    [Range(0.3f, 2f)]
    [SerializeField] private float _obstacleCheckDistance = 0.8f;
    [Range(1f, 15f)]
    [SerializeField] private float _wallScanDistance = 6f;
    [SerializeField] private LayerMask _obstacleLayerMask;

    [Range(60f, 360f)]
    [Tooltip("Velocidad de giro normal hacia el player")]
    [SerializeField] private float _rotationSpeed = 180f;

    [Range(30f, 180f)]
    [Tooltip("Velocidad de giro mientras esquiva")]
    [SerializeField] private float _avoidanceRotationSpeed = 90f;

    [Range(0f, 1f)]
    [Tooltip("0 = sigue la pared puro, 1 = tira hacia el player. Empieza en 0.2")]
    [SerializeField] private float _wallFollowBlend = 0.2f;

    [Header("Referencias")]
    [SerializeField] private BoxDetector2D detector;

    private Vector2 _currentMoveDir;
    private Vector2 _wallSlideDir;
    private Vector2 _wallNormal;
    private bool _isAvoiding = false;

    protected override void Awake()
    {
        base.Awake();
        if (detector == null)
            detector = GetComponent<BoxDetector2D>();
    }

    protected override void UpdateMovement(float distToPlayer)
    {
        if (detector == null || !detector.PlayerDetected)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = DirectionToPlayer();

        // ── Detección de pared ────────────────────────────────────────────────
        // SIEMPRE mira hacia el player para detectar si hay pared en medio.
        // Separado del _currentMoveDir para evitar retroalimentación.
        RaycastHit2D wallHit = Physics2D.CircleCast(
            transform.position, _obstacleCheckCircleRadius,
            toPlayer,                        // <- hacia el player, no hacia _currentMoveDir
            _obstacleCheckDistance, _obstacleLayerMask
        );

        if (wallHit.collider != null)
        {
            // Hay pared entre el enemigo y el player
            if (!_isAvoiding)
            {
                // Primera vez que detecta — elige el lado por el que hay más espacio
                // y que más se acerca al player
                _wallNormal = wallHit.normal;
                Vector2 left = new Vector2(-_wallNormal.y, _wallNormal.x);
                Vector2 right = new Vector2(_wallNormal.y, -_wallNormal.x);

                float scoreLeft = MeasureFreeSpace(left) + Vector2.Dot(left, toPlayer) * 2f;
                float scoreRight = MeasureFreeSpace(right) + Vector2.Dot(right, toPlayer) * 2f;

                _wallSlideDir = scoreLeft > scoreRight ? left : right;
                _isAvoiding = true;
            }

            // Wall following: mezcla el slide con ir al player
            // El blend bajo (0.2) hace que siga la pared casi puro
            // pero con una ligera tracción hacia el player para no quedarse atascado
            Vector2 targetDir = Vector2.Lerp(_wallSlideDir, toPlayer, _wallFollowBlend).normalized;

            _currentMoveDir = Vector2.MoveTowards(
                _currentMoveDir, targetDir,
                _avoidanceRotationSpeed * Mathf.Deg2Rad * Time.deltaTime
            );
        }
        else
        {
            // Sin obstáculo hacia el player — va directo
            _isAvoiding = false;
            _wallSlideDir = Vector2.zero;

            _currentMoveDir = Vector2.MoveTowards(
                _currentMoveDir, toPlayer,
                _rotationSpeed * Mathf.Deg2Rad * Time.deltaTime
            );
        }

        FaceDirection(_currentMoveDir);

        if (distToPlayer > data.attackRange)
            rb.linearVelocity = _currentMoveDir * data.moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    private float MeasureFreeSpace(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, direction, _wallScanDistance, _obstacleLayerMask
        );
        return hit.collider != null ? hit.distance : _wallScanDistance;
    }

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (_currentMoveDir == Vector2.zero) return;

        // Dirección actual
        Gizmos.color = _isAvoiding ? Color.red : Color.yellow;
        Vector3 castEnd = transform.position + (Vector3)(_currentMoveDir * _obstacleCheckDistance);
        Gizmos.DrawWireSphere(castEnd, _obstacleCheckCircleRadius);
        Gizmos.DrawLine(transform.position, castEnd);

        if (_isAvoiding)
        {
            // Normal de la pared (cian)
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, _wallNormal * 0.8f);

            // Dirección de slide elegida (magenta)
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, _wallSlideDir * 1.2f);

            // Cast de detección hacia el player (naranja)
            if (player != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f);
                Gizmos.DrawRay(transform.position, DirectionToPlayer() * _obstacleCheckDistance);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.8f,
                $"Wall Following | blend: {_wallFollowBlend:F2}"
            );
#endif
        }

        // Dirección al player (azul)
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, DirectionToPlayer() * 1.5f);
        }
    }
    #endregion
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ContextSteering : MonoBehaviour
{
    [Header("Direcciones")]
    [SerializeField] private int rayCount = 8;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Strafing (Flanqueo)")]
    [SerializeField] private float strafeRange = 3f;
    [SerializeField] private float strafeStrength = 2f;
    [SerializeField] private float followDistanceVariation = 0.5f;

    [Header("Separación")]
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationStrength = 2f;
    [SerializeField] private float separationAngleOffset = 25f;

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleDetectionRange = 1.5f;

    private float[] interest;
    private float[] danger;
    private Vector2[] directions;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 currentVelocity;
    private float personalFollowDistance;

    // Knockback — controlado desde EnemyBase
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        interest = new float[rayCount];
        danger = new float[rayCount];
        directions = new Vector2[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount) * Mathf.Deg2Rad;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        personalFollowDistance = Random.Range(-followDistanceVariation, followDistanceVariation);
    }

    private void FixedUpdate()
    {
        // Si está en knockback no mueve — EnemyBase gestiona la velocidad
        if (isKnockedBack || player == null) return;

        UpdateInterest();
        UpdateDanger();

        Vector2 desiredDirection = GetBestDirection();
        currentVelocity = Vector2.Lerp(currentVelocity, desiredDirection * moveSpeed, smoothSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;
    }

    // Llamado desde EnemyBase al recibir knockback
    public void SetKnockedBack(bool value)
    {
        isKnockedBack = value;

        // Al salir del knockback resetea la velocidad actual para no acumular
        if (!value) currentVelocity = Vector2.zero;
    }

    private void UpdateInterest()
    {
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position);
        float distToPlayer = toPlayer.magnitude;
        Vector2 toPlayerNorm = toPlayer.normalized;

        for (int i = 0; i < rayCount; i++)
        {
            float dot = Vector2.Dot(directions[i], toPlayerNorm);

            if (distToPlayer < strafeRange + personalFollowDistance)
            {
                float strafeFactor = 1f - Mathf.Abs(dot);
                dot = Mathf.Lerp(dot, strafeFactor * strafeStrength, 0.6f);
            }

            interest[i] = Mathf.Max(0f, dot);
        }
    }

    private void UpdateDanger()
    {
        for (int i = 0; i < rayCount; i++)
        {
            danger[i] = 0f;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], obstacleDetectionRange, obstacleLayer);
            if (hit.collider != null)
            {
                float proximityFactor = 1f - (hit.distance / obstacleDetectionRange);
                danger[i] = proximityFactor;
            }

            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (Collider2D col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                if (!col.TryGetComponent(out ContextSteering _)) continue;

                Vector2 awayFromNeighbor = (Vector2)transform.position - (Vector2)col.transform.position;
                float dist = awayFromNeighbor.magnitude;
                if (dist <= 0) continue;

                float oblique = separationAngleOffset * Mathf.Deg2Rad;
                Vector2 obliqueDir = new Vector2(
                    awayFromNeighbor.x * Mathf.Cos(oblique) - awayFromNeighbor.y * Mathf.Sin(oblique),
                    awayFromNeighbor.x * Mathf.Sin(oblique) + awayFromNeighbor.y * Mathf.Cos(oblique)
                ).normalized;

                float separationDot = Vector2.Dot(directions[i], obliqueDir);
                float separationWeight = (separationStrength / dist) * Mathf.Max(0f, separationDot);
                danger[i] += separationWeight;
            }
        }
    }

    private Vector2 GetBestDirection()
    {
        float bestWeight = float.MinValue;
        int bestIndex = 0;

        for (int i = 0; i < rayCount; i++)
        {
            float weight = interest[i] - danger[i];
            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestIndex = i;
            }
        }

        if (bestWeight < 0f)
        {
            float minDanger = float.MaxValue;
            for (int i = 0; i < rayCount; i++)
            {
                if (danger[i] < minDanger)
                {
                    minDanger = danger[i];
                    bestIndex = i;
                }
            }
        }

        return directions[bestIndex];
    }

    private void OnDrawGizmosSelected()
    {
        if (directions == null) return;

        for (int i = 0; i < rayCount; i++)
        {
            Gizmos.color = Color.green;
            if (interest != null)
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(directions[i] * interest[i]));

            Gizmos.color = Color.red;
            if (danger != null)
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(directions[i] * danger[i] * 0.5f));
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, strafeRange);
    }
}
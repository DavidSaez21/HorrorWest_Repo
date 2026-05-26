using UnityEngine;

/// <summary>
/// Context-Based Steering Behavior para enemigos Top-Down 2D.
/// Evalúa múltiples direcciones radiales y elige la mejor combinando interés y peligro.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ContextSteering : MonoBehaviour
{
    [Header("Direcciones")]
    [SerializeField] private int rayCount = 8;                  // Número de direcciones a evaluar (8 o 16)

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float smoothSpeed = 8f;            // Suavizado del vector de movimiento

    [Header("Strafing (Flanqueo)")]
    [SerializeField] private float strafeRange = 3f;            // Distancia a la que empieza a flanquear
    [SerializeField] private float strafeStrength = 2f;         // Intensidad del flanqueo lateral
    [SerializeField] private float followDistanceVariation = 0.5f; // Variación aleatoria de distancia (anti-simetría)

    [Header("Separación")]
    [SerializeField] private float separationRadius = 1.2f;     // Radio de detección de aliados cercanos
    [SerializeField] private float separationStrength = 2f;     // Fuerza de separación
    [SerializeField] private float separationAngleOffset = 25f; // Ángulo oblicuo anti-jitter (grados)

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleDetectionRange = 1.5f;

    // Arrays de contexto
    private float[] interest;
    private float[] danger;
    private Vector2[] directions;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 currentVelocity;
    private float personalFollowDistance;  // Distancia individual para romper simetría

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Inicializa arrays
        interest = new float[rayCount];
        danger = new float[rayCount];
        directions = new Vector2[rayCount];

        // Precalcula las direcciones radiales
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

        // Variación aleatoria individual para romper la simetría entre enemigos
        personalFollowDistance = Random.Range(-followDistanceVariation, followDistanceVariation);
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        UpdateInterest();
        UpdateDanger();

        Vector2 desiredDirection = GetBestDirection();
        currentVelocity = Vector2.Lerp(currentVelocity, desiredDirection * moveSpeed, smoothSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;
    }

    // ── Interés ───────────────────────────────────────────────────────────────
    private void UpdateInterest()
    {
        Vector2 toPlayer = ((Vector2)player.position - (Vector2)transform.position);
        float distToPlayer = toPlayer.magnitude;
        Vector2 toPlayerNorm = toPlayer.normalized;

        for (int i = 0; i < rayCount; i++)
        {
            float dot = Vector2.Dot(directions[i], toPlayerNorm);

            // Strafing — cuando está cerca aplica función de flanqueo lateral
            if (distToPlayer < strafeRange + personalFollowDistance)
            {
                // Reduce el interés frontal/trasero y favorece los laterales
                // dot cercano a 0 = lateral → alto interés; dot cercano a 1 = frontal → bajo interés
                float strafeFactor = 1f - Mathf.Abs(dot);
                dot = Mathf.Lerp(dot, strafeFactor * strafeStrength, 0.6f);
            }

            interest[i] = Mathf.Max(0f, dot);
        }
    }

    // ── Peligro ───────────────────────────────────────────────────────────────
    private void UpdateDanger()
    {
        for (int i = 0; i < rayCount; i++)
        {
            danger[i] = 0f;

            // Obstáculos — raycast en cada dirección
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directions[i], obstacleDetectionRange, obstacleLayer);
            if (hit.collider != null)
            {
                // Más cerca = más peligro
                float proximityFactor = 1f - (hit.distance / obstacleDetectionRange);
                danger[i] = proximityFactor;
            }

            // Separación de otros enemigos con ángulo oblicuo anti-jitter
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (Collider2D col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                if (!col.TryGetComponent(out ContextSteering _)) continue;

                Vector2 awayFromNeighbor = (Vector2)transform.position - (Vector2)col.transform.position;
                float dist = awayFromNeighbor.magnitude;
                if (dist <= 0) continue;

                // Aplica ángulo oblicuo para evitar jitter
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

    // ── Decisión final ────────────────────────────────────────────────────────
    private Vector2 GetBestDirection()
    {
        Vector2 bestDir = Vector2.zero;
        float bestWeight = float.MinValue;

        for (int i = 0; i < rayCount; i++)
        {
            float weight = interest[i] - danger[i];

            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestDir = directions[i];
            }
        }

        return bestDir;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (directions == null) return;

        for (int i = 0; i < rayCount; i++)
        {
            // Verde = interés, Rojo = peligro
            Gizmos.color = Color.green;
            if (interest != null)
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(directions[i] * interest[i]));

            Gizmos.color = Color.red;
            if (danger != null)
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(directions[i] * danger[i]));
        }

        // Radio de separación
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        // Radio de strafing
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, strafeRange);
    }
}
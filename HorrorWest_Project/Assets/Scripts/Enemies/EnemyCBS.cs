using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCBS : MonoBehaviour
{
    [Header("CBS")]
    [Range(8, 32)]
    public int rayCount = 16;
    public float dangerRadius = 3f;
    public float enemySeparationRadius = 1.2f;
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    [Header("Suavizado")]
    [Range(0f, 1f)]
    public float directionSmoothing = 0.15f;

    [Header("Wall Steering")]
    public float wallSteerRadius = 4f;
    [Range(0f, 2f)]
    public float lateralBonus = 0.8f;

    [Header("Debug")]
    public bool showDebug = true;

    private Vector2 smoothedDir = Vector2.zero;
    private float[] interest;
    private float[] danger;
    private Vector2[] directions;
    private Vector2 debugDesiredDir;

    private void Awake()
    {
        interest = new float[rayCount];
        danger = new float[rayCount];
        directions = new Vector2[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount) * Mathf.Deg2Rad;
            directions[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    public Vector2 GetBestDirection(Vector2 desiredDir)
    {
        debugDesiredDir = desiredDir;

        // 1 — Interés base
        for (int i = 0; i < rayCount; i++)
            interest[i] = Mathf.Max(0f, Vector2.Dot(directions[i], desiredDir));

        // 2 — Peligro
        bool wallAhead = false;
        for (int i = 0; i < rayCount; i++)
        {
            danger[i] = 0f;

            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, directions[i], dangerRadius, obstacleLayer);
            if (wallHit.collider != null)
            {
                float proximity = 1f - (wallHit.distance / dangerRadius);
                danger[i] = proximity;
            }

            RaycastHit2D farHit = Physics2D.Raycast(transform.position, directions[i], wallSteerRadius, obstacleLayer);
            if (farHit.collider != null && Vector2.Dot(directions[i], desiredDir) > 0.3f)
                wallAhead = true;

            RaycastHit2D enemyHit = Physics2D.Raycast(transform.position, directions[i], enemySeparationRadius, enemyLayer);
            if (enemyHit.collider != null && enemyHit.collider.gameObject != gameObject)
            {
                float proximity = 1f - (enemyHit.distance / enemySeparationRadius);
                danger[i] = Mathf.Max(danger[i], proximity * 0.6f);
            }
        }

        // 3 — Bonus lateral solo si hay pared adelante
        if (wallAhead)
        {
            for (int i = 0; i < rayCount; i++)
            {
                float lateral = 1f - Mathf.Abs(Vector2.Dot(directions[i], desiredDir));
                interest[i] += lateral * lateralBonus;
            }
        }

        // 4 — Anula peligrosas
        for (int i = 0; i < rayCount; i++)
        {
            if (danger[i] >= 0.5f)
                interest[i] = 0f;
        }

        // 5 — Mejor dirección
        int bestIndex = 0;
        float bestScore = float.MinValue;
        for (int i = 0; i < rayCount; i++)
        {
            if (interest[i] > bestScore)
            {
                bestScore = interest[i];
                bestIndex = i;
            }
        }

        // 6 — Fallback mejorado: combina peligro mínimo CON algo de interés hacia el player
        // En lugar de solo buscar menos peligro, busca el mejor balance
        if (bestScore <= 0f)
        {
            float bestFallback = float.MinValue;
            for (int i = 0; i < rayCount; i++)
            {
                // Score = alineación con player - peligro
                // Así no se aleja nunca completamente
                float dot = Vector2.Dot(directions[i], desiredDir);
                float fallbackScore = dot - danger[i] * 2f;

                if (fallbackScore > bestFallback)
                {
                    bestFallback = fallbackScore;
                    bestIndex = i;
                }
            }
        }

        Vector2 bestDir = directions[bestIndex];
        smoothedDir = Vector2.Lerp(smoothedDir, bestDir, 1f - directionSmoothing).normalized;
        return smoothedDir;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying) return;
        if (directions == null) return;

        for (int i = 0; i < rayCount; i++)
        {
            if (danger != null && danger[i] >= 0.5f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, directions[i] * dangerRadius);
            }
            else if (interest != null)
            {
                float t = Mathf.Clamp01(interest[i]);
                Gizmos.color = Color.Lerp(Color.gray, Color.green, t);
                Gizmos.DrawRay(transform.position, directions[i] * Mathf.Lerp(0.2f, dangerRadius, t));
            }
        }

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, dangerRadius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, wallSteerRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, debugDesiredDir * dangerRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, smoothedDir * dangerRadius * 1.2f);
    }
}
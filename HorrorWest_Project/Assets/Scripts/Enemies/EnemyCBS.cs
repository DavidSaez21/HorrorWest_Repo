using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCBS : MonoBehaviour
{
    [Header("CBS")]
    [Range(8, 32)]
    public int rayCount = 16;
    public float dangerRadius = 2f;
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    [Header("Suavizado")]
    [Range(0f, 1f)]
    public float directionSmoothing = 0.6f;

    [Header("Debug")]
    public bool showDebug = true;

    private Vector2 smoothedDir = Vector2.zero;
    private Vector2[] debugDirs;
    private float[] debugScores;
    private Vector2 debugDesiredDir;

    private void OnEnable()
    {
        smoothedDir = Vector2.zero;
    }

    public Vector2 GetBestDirection(Vector2 desiredDir)
    {
        float bestScore = float.MinValue;
        Vector2 bestDir = desiredDir;
        float angleStep = 360f / rayCount;

        if (debugDirs == null || debugDirs.Length != rayCount)
        {
            debugDirs = new Vector2[rayCount];
            debugScores = new float[rayCount];
        }

        debugDesiredDir = desiredDir;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            // Raycast obstáculos
            RaycastHit2D wallHit = Physics2D.Raycast(
                transform.position, dir, dangerRadius, obstacleLayer
            );

            // Raycast otros enemigos
            RaycastHit2D enemyHit = Physics2D.Raycast(
                transform.position, dir, dangerRadius * 0.6f, enemyLayer
            );

            float score;

            if (wallHit.collider != null)
            {
                // Dirección bloqueada por pared
                float proximity = 1f - (wallHit.distance / dangerRadius);
                score = -proximity * 2f;
            }
            else
            {
                // Dirección libre: puntúa por alineación con el deseo
                score = Vector2.Dot(dir, desiredDir);
            }

            // Penalización adicional por enemigos cercanos (no bloquea, solo desvía)
            if (enemyHit.collider != null && enemyHit.collider.gameObject != gameObject)
            {
                float proximity = 1f - (enemyHit.distance / (dangerRadius * 0.6f));
                score -= proximity * 1.2f;
            }

            debugDirs[i] = dir;
            debugScores[i] = score;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        smoothedDir = Vector2.Lerp(bestDir, smoothedDir, directionSmoothing).normalized;
        return smoothedDir;
    }

    // OnDrawGizmos (sin Selected) para que se vea siempre en Play Mode
    private void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying) return;
        if (debugDirs == null) return;

        float minS = float.MaxValue, maxS = float.MinValue;
        foreach (float s in debugScores)
        {
            if (s < minS) minS = s;
            if (s > maxS) maxS = s;
        }
        float range = Mathf.Max(maxS - minS, 0.001f);

        for (int i = 0; i < rayCount; i++)
        {
            float t = (debugScores[i] - minS) / range;
            Gizmos.color = Color.Lerp(Color.red, Color.green, t);
            float length = Mathf.Lerp(0.3f, dangerRadius, t);
            Gizmos.DrawRay(transform.position, debugDirs[i] * length);
        }

        // Radio CBS en cyan
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, dangerRadius);

        // Dirección deseada en azul
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, debugDesiredDir * dangerRadius);
    }
}
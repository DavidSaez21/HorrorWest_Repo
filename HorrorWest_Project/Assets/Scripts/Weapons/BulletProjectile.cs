using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletProjectile : MonoBehaviour
{
    private float damage;
    private Rigidbody2D rb;
    private Vector2 travelDirection;
    private int pierceCount = 0;
    private int maxPierce = 0;
    private bool isLastBullet = false;

    [Header("Plague Settings")]
    [SerializeField] private float plagueDamagePerSecond = 5f;
    [SerializeField] private float plagueDuration = 3f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(Vector2 direction, float speed, float damage, float range)
    {
        this.damage = damage;
        this.travelDirection = direction.normalized;
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, range / speed);

        if (PlayerStats.Instance != null)
        {
            float sizeMultiplier = 1f + PlayerStats.Instance.GetBulletSizeBonus();
            transform.localScale *= sizeMultiplier;

            if (PlayerStats.Instance.hasPiercingBullets)
                maxPierce = 1;
        }
    }

    public void SetLastBullet(bool value) => isLastBullet = value;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            if (PlayerStats.Instance != null && PlayerStats.Instance.hasExecute)
            {
                if (other.TryGetComponent(out EnemyBase enemyExec))
                {
                    if (enemyExec.GetHealthPercent() < PlayerStats.Instance.GetExecuteThreshold())
                    {
                        enemyExec.TakeDamage(999999f, travelDirection);
                        TryExplode();
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            float finalDamage = PlayerStats.Instance != null
                ? PlayerStats.Instance.CalculateDamage(damage)
                : damage;

            if (isLastBullet && PlayerStats.Instance != null && PlayerStats.Instance.hasLastBullet)
                finalDamage *= 3f;

            if (other.TryGetComponent(out EnemyBase enemy))
                enemy.TakeDamage(finalDamage, travelDirection);
            else
                target.TakeDamage(finalDamage);

            if (PlayerStats.Instance != null && (PlayerStats.Instance.hasPlagueBullets || PlayerStats.Instance.hasTotalPlague))
            {
                if (!other.TryGetComponent(out PlagueEffect _))
                {
                    PlagueEffect plague = other.gameObject.AddComponent<PlagueEffect>();
                    plague.Init(plagueDamagePerSecond, plagueDuration);
                }
            }

            if (pierceCount < maxPierce)
            {
                pierceCount++;
                return;
            }
        }

        TryExplode();
        Destroy(gameObject);
    }

    private void TryExplode()
    {
        if (PlayerStats.Instance == null || !PlayerStats.Instance.hasExplosiveBullets) return;

        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;
        ExplosionEffect explosion = explosionObj.AddComponent<ExplosionEffect>();
        explosion.Init(explosionRadius, explosionDamage, enemyLayer);
    }

    // Muestra el radio de explosión en el prefab desde el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
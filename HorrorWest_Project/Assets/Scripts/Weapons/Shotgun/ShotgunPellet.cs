using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShotgunPellet : MonoBehaviour
{
    private float maxDamage;
    private float maxRange;
    private float speed;
    private Rigidbody2D rb;
    private Vector2 spawnPosition;
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

    public void Init(Vector2 direction, float speed, float maxDamage, float maxRange)
    {
        this.maxDamage = maxDamage;
        this.maxRange = maxRange;
        this.speed = speed;
        this.travelDirection = direction.normalized;

        spawnPosition = transform.position;
        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, maxRange / speed);

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

            float distanceTravelled = Vector2.Distance(spawnPosition, transform.position);
            float distanceRatio = Mathf.Clamp01(distanceTravelled / maxRange);
            float damageFalloff = 1f - Mathf.Pow(distanceRatio, 0.5f);
            float baseDmg = Mathf.Max(1f, maxDamage * damageFalloff);

            float finalDamage = PlayerStats.Instance != null
                ? PlayerStats.Instance.CalculateDamage(baseDmg)
                : baseDmg;

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
}
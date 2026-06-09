using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Damage Falloff")]
    [SerializeField] private bool useDamageFalloff = false;

    [Header("Plague Settings")]
    [SerializeField] private float plagueDamagePerSecond = 5f;
    [SerializeField] private float plagueDuration = 3f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject explosionVFXPrefab;

    private float damage;
    private float maxRange;
    private Vector2 spawnPosition;
    private Vector2 travelDirection;
    private int pierceCount = 0;
    private int maxPierce = 0;
    private bool isLastBullet = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(Vector2 direction, float speed, float damage, float range)
    {
        this.damage = damage;
        this.maxRange = range;
        this.travelDirection = direction.normalized;
        this.spawnPosition = transform.position;

        rb.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, range / speed);

        if (PlayerManager.Instance.Stats != null)
        {
            float sizeMultiplier = 1f + PlayerManager.Instance.Stats.GetBulletSizeBonus();
            transform.localScale *= sizeMultiplier;

            if (PlayerManager.Instance.Stats.hasPiercingBullets)
                maxPierce = 1;
        }
    }

    public void SetLastBullet(bool value) => isLastBullet = value;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out IDamageable target)) return;

        if (TryExecute(other)) return;

        float finalDamage = CalculateFinalDamage();

        if (other.TryGetComponent(out EnemyBase enemy))
            enemy.TakeDamage(finalDamage, travelDirection);
        else
            target.TakeDamage(finalDamage);

        TryApplyPlague(other);

        if (pierceCount < maxPierce)
        {
            pierceCount++;
            return;
        }

        TryExplode();
        Destroy(gameObject);
    }

    private float CalculateFinalDamage()
    {
        float baseDmg = damage;

        if (useDamageFalloff)
        {
            float distanceTravelled = Vector2.Distance(spawnPosition, transform.position);
            float distanceRatio = Mathf.Clamp01(distanceTravelled / maxRange);
            float falloff = 1f - Mathf.Pow(distanceRatio, 0.5f);
            baseDmg = Mathf.Max(1f, damage * falloff);
        }

        float finalDamage = PlayerManager.Instance.Stats != null
            ? PlayerManager.Instance.Stats.CalculateDamage(baseDmg)
            : baseDmg;

        if (isLastBullet && PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasLastBullet)
            finalDamage *= 3f;

        return finalDamage;
    }

    private bool TryExecute(Collider2D other)
    {
        if (PlayerManager.Instance.Stats == null || !PlayerManager.Instance.Stats.hasExecute) return false;
        if (!other.TryGetComponent(out EnemyBase enemy)) return false;
        if (enemy.GetHealthPercent() >= PlayerManager.Instance.Stats.GetExecuteThreshold()) return false;

        enemy.TakeDamage(999999f, travelDirection);
        TryExplode();
        Destroy(gameObject);
        return true;
    }

    private void TryApplyPlague(Collider2D other)
    {
        if (PlayerManager.Instance.Stats == null) return;
        if (!PlayerManager.Instance.Stats.hasPlagueBullets && !PlayerManager.Instance.Stats.hasTotalPlague) return;

        if (!other.TryGetComponent(out PlagueEffect _))
        {
            PlagueEffect plague = other.gameObject.AddComponent<PlagueEffect>();
            plague.Init(plagueDamagePerSecond, plagueDuration);
        }
    }

    private void TryExplode()
    {
        if (PlayerManager.Instance.Stats == null || !PlayerManager.Instance.Stats.hasExplosiveBullets) return;

        // VFX de explosión
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;
        ExplosionEffect explosion = explosionObj.AddComponent<ExplosionEffect>();
        explosion.Init(explosionRadius, explosionDamage, enemyLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
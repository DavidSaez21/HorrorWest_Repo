using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Projectile — unifica BulletProjectile y ShotgunPellet.
//
// La única diferencia real entre ambos era el falloff de daño por distancia
// de la escopeta. Aquí se controla con el flag useDamageFalloff.
//
// MIGRACIÓN:
//   - Prefab de bala del revólver/rifle:  useDamageFalloff = false
//   - Prefab de perdigón de escopeta:     useDamageFalloff = true
// ─────────────────────────────────────────────────────────────────────────────
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // ── Falloff (solo escopeta) ───────────────────────────────────────────────
    [Header("Damage Falloff")]
    [Tooltip("Actívalo en los perdigones de escopeta. Desactívalo en bala normal.")]
    [SerializeField] private bool useDamageFalloff = false;

    // ── Plague ────────────────────────────────────────────────────────────────
    [Header("Plague Settings")]
    [SerializeField] private float plagueDamagePerSecond = 5f;
    [SerializeField] private float plagueDuration = 3f;

    // ── Explosion ─────────────────────────────────────────────────────────────
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;

    // ── Estado interno ────────────────────────────────────────────────────────
    private float damage;
    private float maxRange;
    private Vector2 spawnPosition;
    private Vector2 travelDirection;
    private int pierceCount = 0;
    private int maxPierce = 0;
    private bool isLastBullet = false;
    private Rigidbody2D rb;

    // ── Init ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    /// <summary>
    /// Inicializa el proyectil. Llamado desde cada arma al instanciar.
    /// </summary>
    public void Init(Vector2 direction, float speed, float damage, float range)
    {
        this.damage = damage;
        this.maxRange = range;
        this.travelDirection = direction.normalized;
        this.spawnPosition = transform.position;

        rb.linearVelocity = direction.normalized * speed;

        // Auto-destrucción al salir del rango
        Destroy(gameObject, range / speed);

        // Aplica stats del jugador al inicializar
        if (PlayerManager.Instance.Stats != null)
        {
            float sizeMultiplier = 1f + PlayerManager.Instance.Stats.GetBulletSizeBonus();
            transform.localScale *= sizeMultiplier;

            if (PlayerManager.Instance.Stats.hasPiercingBullets)
                maxPierce = 1;
        }
    }

    public void SetLastBullet(bool value) => isLastBullet = value;

    // ── Colisión ──────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out IDamageable target)) return;

        // Execute: mata instantáneamente si el enemigo está por debajo del umbral
        if (TryExecute(other))
            return;

        // Cálculo de daño
        float finalDamage = CalculateFinalDamage();

        // Aplica daño
        if (other.TryGetComponent(out EnemyBase enemy))
            enemy.TakeDamage(finalDamage, travelDirection);
        else
            target.TakeDamage(finalDamage);

        // Plague
        TryApplyPlague(other);

        // Piercing: si aún quedan perforaciones disponibles, no destruir
        if (pierceCount < maxPierce)
        {
            pierceCount++;
            return;
        }

        TryExplode();
        Destroy(gameObject);
    }

    // ── Helpers de daño ───────────────────────────────────────────────────────
    private float CalculateFinalDamage()
    {
        float baseDmg = damage;

        // Falloff por distancia (escopeta)
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

        // Last Bullet: x3 de daño en la última bala del cargador
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

        // Evita añadir el componente si ya tiene plague activo
        if (!other.TryGetComponent(out PlagueEffect _))
        {
            PlagueEffect plague = other.gameObject.AddComponent<PlagueEffect>();
            plague.Init(plagueDamagePerSecond, plagueDuration);
        }
    }

    private void TryExplode()
    {
        if (PlayerManager.Instance.Stats == null || !PlayerManager.Instance.Stats.hasExplosiveBullets) return;

        GameObject explosionObj = new GameObject("Explosion");
        explosionObj.transform.position = transform.position;
        ExplosionEffect explosion = explosionObj.AddComponent<ExplosionEffect>();
        explosion.Init(explosionRadius, explosionDamage, enemyLayer);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
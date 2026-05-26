using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShotgunPellet : MonoBehaviour
{
    private float maxDamage;
    private float maxRange;
    private float speed;
    private Rigidbody2D rb;
    private Vector2 spawnPosition;

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

        spawnPosition = transform.position;
        rb.linearVelocity = direction.normalized * speed;

        float lifetime = maxRange / speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            float distanceTravelled = Vector2.Distance(spawnPosition, transform.position);
            float distanceRatio = Mathf.Clamp01(distanceTravelled / maxRange);
            float damageFalloff = 1f - Mathf.Pow(distanceRatio, 0.5f);
            float baseDmg = Mathf.Max(1f, maxDamage * damageFalloff);

            // Aplica crítico sobre el daño ya calculado con falloff
            float finalDamage = PlayerStats.Instance != null
                ? PlayerStats.Instance.CalculateDamage(baseDmg)
                : baseDmg;

            target.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }
}
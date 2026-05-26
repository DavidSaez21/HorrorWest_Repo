using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletProjectile : MonoBehaviour
{
    private float damage;
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
        rb.linearVelocity = direction.normalized * speed;

        float lifetime = range / speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent(out IDamageable target))
        {
            // Calcula daño con posible crítico
            float finalDamage = PlayerStats.Instance != null
                ? PlayerStats.Instance.CalculateDamage(damage)
                : damage;

            target.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }
}
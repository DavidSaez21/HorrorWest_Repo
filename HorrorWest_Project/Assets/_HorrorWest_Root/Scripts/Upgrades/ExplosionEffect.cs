using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDamage = 15f;
    [SerializeField] private LayerMask enemyLayer;

    public void Init(float radius, float damage, LayerMask enemyLayer)
    {
        this.explosionRadius = radius;
        this.explosionDamage = damage;
        this.enemyLayer = enemyLayer;
        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out EnemyBase enemy))
            {
                Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                enemy.TakeDamage(explosionDamage, knockbackDir);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
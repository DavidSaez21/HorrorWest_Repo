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
            // Calcula cuánto ha viajado el perdigón (0 = origen, 1 = rango máximo)
            float distanceTravelled = Vector2.Distance(spawnPosition, transform.position);
            float distanceRatio = Mathf.Clamp01(distanceTravelled / maxRange);

            // A más distancia, menos daño — cae en curva para que sea más pronunciado
            float damageFalloff = 1f - Mathf.Pow(distanceRatio, 0.5f);
            float finalDamage = Mathf.Max(1f, maxDamage * damageFalloff);

            target.TakeDamage(finalDamage);
        }

        Destroy(gameObject);
    }
}
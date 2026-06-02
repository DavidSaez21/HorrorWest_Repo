using UnityEngine;

/// <summary>
/// Proyectil del escupitajo — viaja en línea recta hacia la posición
/// predicha del player. Al impactar hace daño en área.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SpitProjectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private GameObject impactVFXPrefab;

    private float _damage;
    private float _radius;
    private LayerMask _playerLayer;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(Vector2 direction, float damage, float radius)
    {
        _damage = damage;
        _radius = radius;
        _playerLayer = LayerMask.GetMask("Player");

        _rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            ph?.TakeDamage(_damage);

            if (impactVFXPrefab != null)
                Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
            return;
        }

        // Impacta con obstáculos también
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Obstacle")) != 0)
        {
            if (impactVFXPrefab != null)
                Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
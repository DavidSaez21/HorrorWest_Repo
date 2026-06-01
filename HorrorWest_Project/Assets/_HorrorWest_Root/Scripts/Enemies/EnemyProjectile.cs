using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _range = 12f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        // Move in the direction the prefab is facing (set by EnemyShooter on Instantiate)
        Vector2 dir = transform.right; // right = forward for angle-based rotation
        _rb.linearVelocity = dir * _speed;

        Destroy(gameObject, _range / _speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore other enemies and enemy projectiles
        if (other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out PlayerHealth ph))
            ph.TakeDamage(_damage);

        Destroy(gameObject);
    }
}
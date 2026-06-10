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
        // Usa up en lugar de right — la rotación del juego usa -90 como base
        Vector2 dir = transform.up;
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
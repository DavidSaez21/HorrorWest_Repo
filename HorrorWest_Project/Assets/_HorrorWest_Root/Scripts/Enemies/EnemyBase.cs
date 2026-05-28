using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("Drops")]
    [SerializeField] protected GameObject[] coinPrefabs;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float knockbackDuration = 0.15f;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform player;
    protected bool isDead = false;
    protected Rigidbody2D rb;

    private bool isKnockedBack = false;
    private bool hasDetectedPlayer = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackVelocity;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    protected virtual void Start()
    {
        currentHealth = data.maxHealth;

        rb.excludeLayers = LayerMask.GetMask("Player"); // <- aquí

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if (isKnockedBack) return;

        float dist = DistanceToPlayer();

        // Una vez detectado, persigue para siempre
        if (!hasDetectedPlayer && dist <= data.detectionRange)
            hasDetectedPlayer = true;

        if (hasDetectedPlayer)
            UpdateMovement(dist);
        else
            rb.linearVelocity = Vector2.zero;

        if (dist <= data.attackRange && CanAttack())
        {
            PerformAttack();
            ResetAttackCooldown();
        }
    }

    private void FixedUpdate()
    {
        if (!isKnockedBack) return;

        knockbackTimer -= Time.fixedDeltaTime;
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
        rb.linearVelocity = knockbackVelocity;

        if (knockbackTimer <= 0f)
        {
            isKnockedBack = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected abstract void UpdateMovement(float distToPlayer);

    protected virtual void PerformAttack()
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(data.damage);
    }

    public void TakeDamage(float amount) => TakeDamage(amount, Vector2.zero);

    public virtual void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, amount - data.defense);
        currentHealth -= finalDamage;

        if (hitDirection != Vector2.zero)
        {
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
            knockbackVelocity = hitDirection.normalized * knockbackForce;
            rb.linearVelocity = knockbackVelocity;
        }

        OnDamageReceived(finalDamage);
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        ExperienceManager.Instance?.AddXP(data.xpReward);
        TryDropCoin();
        TryInstantReload();
        OnDeath();
        Destroy(gameObject);
    }

    protected virtual void OnDamageReceived(float damage) { }
    protected virtual void OnDeath() { }

    protected float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)(player.position - transform.position)).normalized;
    }

    protected void Move(Vector2 direction)
        => rb.linearVelocity = direction * data.moveSpeed;

    protected void FaceDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;
    protected void ResetAttackCooldown() => lastAttackTime = Time.time;
    public float GetHealthPercent() => currentHealth / data.maxHealth;

    private void TryInstantReload()
    {
        if (PlayerManager.Instance.Stats == null) return;
        if (!PlayerManager.Instance.Stats.hasReloadOnKill) return;
        if (Random.value > PlayerManager.Instance.Stats.GetReloadOnKillChance()) return;

        PlayerShoot playerShoot = FindFirstObjectByType<PlayerShoot>();
        if (playerShoot == null) return;

        WeaponBase weapon = playerShoot.GetCurrentWeapon();
        if (weapon is Revolver revolver) revolver.InstantReload();
        else if (weapon is Shotgun shotgun) shotgun.InstantReload();
        else if (weapon is Rifle rifle) rifle.InstantReload();
    }

    private void TryDropCoin()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0) return;
        if (Random.value > data.dropChance) return;

        float total = data.dropChanceCoinOne + data.dropChanceCoinFive + data.dropChanceCoinTen;
        float roll = Random.Range(0f, total);

        int index;
        if (roll < data.dropChanceCoinOne) index = 0;
        else if (roll < data.dropChanceCoinOne + data.dropChanceCoinFive) index = 1;
        else index = 2;

        index = Mathf.Clamp(index, 0, coinPrefabs.Length - 1);
        if (coinPrefabs[index] != null)
            Instantiate(coinPrefabs[index], transform.position, Quaternion.identity);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}
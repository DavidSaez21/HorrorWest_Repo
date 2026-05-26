using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("References")]
    [SerializeField] protected GameObject[] coinPrefabs;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float knockbackDuration = 0.15f;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform player;
    protected bool isDead = false;

    private Rigidbody2D rb;
    private ContextSteering steering;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackVelocity;

    protected virtual void Start()
    {
        currentHealth = data.maxHealth;
        rb = GetComponent<Rigidbody2D>();
        steering = GetComponent<ContextSteering>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            // Va frenando progresivamente hasta parar
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, 10f * Time.fixedDeltaTime);
            rb.linearVelocity = knockbackVelocity;

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = Vector2.zero;
                if (steering != null) steering.SetKnockedBack(false);
            }
        }
    }

    public void TakeDamage(float amount) => TakeDamage(amount, Vector2.zero);

    public void TakeDamage(float amount, Vector2 hitDirection)
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
            if (steering != null) steering.SetKnockedBack(true);
        }

        OnDamageReceived(finalDamage);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        ExperienceManager.Instance?.AddXP(data.xpReward);
        TryDropCoin();
        TryInstantReload();
        OnDeath();
        Destroy(gameObject);
    }

    private void TryInstantReload()
    {
        if (PlayerStats.Instance == null) return;
        if (!PlayerStats.Instance.hasReloadOnKill) return;
        if (Random.value > PlayerStats.Instance.GetReloadOnKillChance()) return;

        PlayerShoot playerShoot = FindFirstObjectByType<PlayerShoot>();
        if (playerShoot == null) return;

        WeaponBase weapon = playerShoot.GetCurrentWeapon();
        if (weapon is Revolver revolver) revolver.InstantReload();
        else if (weapon is Shotgun shotgun) shotgun.InstantReload();
        else if (weapon is Rifle rifle) rifle.InstantReload();
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
        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    protected bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;
    protected void ResetAttackCooldown() => lastAttackTime = Time.time;
    public float GetHealthPercent() => currentHealth / data.maxHealth;

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
}
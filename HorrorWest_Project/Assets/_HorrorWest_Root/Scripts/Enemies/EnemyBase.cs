using UnityEngine;

/// <summary>
/// Base class for all enemies.
/// 
/// Movement philosophy (Vampire Survivors / Hotline Miami):
///   - Enemies always know where the player is. No detection range, no line of sight.
///   - All movement is resolved as steering forces in FixedUpdate.
///   - Enemies never push the player (rb.excludeLayers includes Player).
///   - Separation between enemies is a soft steering force, never a hard velocity override.
///   - Knockback is the only hard velocity override and blocks the steering pipeline.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("Drops")]
    [SerializeField] protected GameObject[] coinPrefabs;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.18f;

    [Header("Separation")]
    [Tooltip("Overlap radius for enemy-to-enemy separation checks. Keep smaller than sprite so clusters look dense.")]
    [Range(0.2f, 2f)]
    [SerializeField] private float separationRadius = 0.55f;

    [Tooltip("Max separation force magnitude. Keeps enemies from stacking without bouncing.")]
    [Range(0.5f, 8f)]
    [SerializeField] private float separationForce = 2f;

    // ── State ─────────────────────────────────────────────────────────────────

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform player;
    protected bool isDead;
    protected Rigidbody2D rb;

    // Knockback
    private bool _isKnockedBack;
    private float _knockbackTimer;
    private Vector2 _knockbackVelocity;

    // Steering
    // Subclasses write their desired move velocity here every FixedUpdate.
    // EnemyBase then blends separation on top and applies the result.
    protected Vector2 DesiredVelocity { get; set; }

    // Separation buffer (reused to avoid per-frame allocation)
    private readonly Collider2D[] _separationBuffer = new Collider2D[16];
    private int _enemyLayerMask;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        _enemyLayerMask = LayerMask.GetMask("Enemy");
    }

    protected virtual void Start()
    {
        currentHealth = data.maxHealth;
        //rb.excludeLayers = LayerMask.GetMask("Player"); // enemies never push the player

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if (_isKnockedBack) return;

        // Attack check (can run every frame — uses a cooldown gate)
        if (DistanceToPlayer() <= data.attackRange && CanAttack())
        {
            PerformAttack();
            ResetAttackCooldown();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        // ── Knockback override — highest priority ──────────────────────────
        if (_isKnockedBack)
        {
            _knockbackTimer -= Time.fixedDeltaTime;
            _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, 12f * Time.fixedDeltaTime);
            rb.linearVelocity = _knockbackVelocity;

            if (_knockbackTimer <= 0f)
            {
                _isKnockedBack = false;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (player == null) return;

        // ── Normal steering pipeline ───────────────────────────────────────
        // 1. Subclass writes DesiredVelocity in UpdateSteering().
        UpdateSteering();

        // 2. Add separation force on top of desired velocity.
        Vector2 finalVelocity = DesiredVelocity + ComputeSeparationForce();

        // 3. Clamp to max speed so separation doesn't over-accelerate.
        if (finalVelocity.magnitude > data.moveSpeed * 1.4f)
            finalVelocity = finalVelocity.normalized * data.moveSpeed * 1.4f;

        rb.linearVelocity = finalVelocity;
    }

    // ── Abstract / virtual API for subclasses ─────────────────────────────────

    /// <summary>
    /// Called every FixedUpdate (while alive and not knocked back).
    /// Subclass must set <see cref="DesiredVelocity"/> here.
    /// Do NOT write to rb.linearVelocity directly — EnemyBase handles that.
    /// </summary>
    protected abstract void UpdateSteering();

    protected virtual void PerformAttack()
    {
        if (player.TryGetComponent(out PlayerHealth ph))
            ph.TakeDamage(data.damage);
    }

    // ── Separation (Vampire Survivors style) ──────────────────────────────────

    /// <summary>
    /// Returns a steering force that nudges this enemy away from overlapping neighbours.
    /// Force is proportional to overlap depth — very light touch so clusters look natural.
    /// Player is on a different layer so it is never included.
    /// </summary>
    private Vector2 ComputeSeparationForce()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position, separationRadius,
            _separationBuffer, _enemyLayerMask);

        Vector2 steer = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _separationBuffer[i];
            if (col == null || col.gameObject == gameObject) continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            float dist = away.magnitude;

            if (dist < 0.001f)
            {
                // Perfectly overlapping — use a deterministic pseudo-random offset
                float a = GetInstanceID() * 0.1f;
                away = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                dist = 0.001f;
            }

            // Overlap depth → 0..1 weight; closer neighbours push harder
            float weight = 1f - Mathf.Clamp01(dist / separationRadius);
            steer += away.normalized * (weight * separationForce);
        }

        return steer;
    }

    // ── Damage / death ────────────────────────────────────────────────────────

    public void TakeDamage(float amount) => TakeDamage(amount, Vector2.zero);

    public virtual void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, amount - data.defense);
        currentHealth -= finalDamage;

        if (hitDirection != Vector2.zero)
        {
            _isKnockedBack = true;
            _knockbackTimer = knockbackDuration;
            _knockbackVelocity = hitDirection.normalized * knockbackForce;
            rb.linearVelocity = _knockbackVelocity;
        }

        OnDamageReceived(finalDamage);

        if (currentHealth <= 0f)
            Die();
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

    // ── Utilities for subclasses ──────────────────────────────────────────────

    protected float DistanceToPlayer()
        => player == null ? Mathf.Infinity : Vector2.Distance(transform.position, player.position);

    protected Vector2 DirectionToPlayer()
        => player == null ? Vector2.zero : ((Vector2)(player.position - transform.position)).normalized;

    /// <summary>Rotates the sprite to face <paramref name="direction"/> (up = forward convention).</summary>
    protected void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;
    protected void ResetAttackCooldown() => lastAttackTime = Time.time;

    public float GetHealthPercent() => currentHealth / data.maxHealth;

    // ── Reward helpers ────────────────────────────────────────────────────────

    private void TryInstantReload()
    {
        if (PlayerManager.Instance?.Stats == null) return;
        if (!PlayerManager.Instance.Stats.hasReloadOnKill) return;
        if (Random.value > PlayerManager.Instance.Stats.GetReloadOnKillChance()) return;

        PlayerShoot playerShoot = FindFirstObjectByType<PlayerShoot>();
        if (playerShoot == null) return;

        WeaponBase weapon = playerShoot.GetCurrentWeapon();
        if (weapon is Revolver r) r.InstantReload();
        else if (weapon is Shotgun s) s.InstantReload();
        else if (weapon is Rifle f) f.InstantReload();
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

    // ── Debug gizmos ──────────────────────────────────────────────────────────

    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
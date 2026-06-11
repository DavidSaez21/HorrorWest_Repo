using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("Animation")]
    [SerializeField] protected Animator[] animators;

    [Header("Drops")]
    [SerializeField] protected GameObject[] coinPrefabs;

    [Header("VFX")]
    [SerializeField] private GameObject bloodSplatterPrefab;
    [SerializeField] private GameObject plagueVFXPrefab;
    [SerializeField] private Transform plagueVFXSpawnPoint;
    [SerializeField] private Material plagueMaterial;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.18f;

    [Header("Separation")]
    [Range(0.2f, 2f)]
    [SerializeField] private float separationRadius = 0.55f;
    [Range(0.5f, 8f)]
    [SerializeField] private float separationForce = 2f;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform player;
    protected bool isDead;
    protected Rigidbody2D rb;

    private bool _isKnockedBack;
    private float _knockbackTimer;
    private Vector2 _knockbackVelocity;
    private HitFlash _hitFlash;

    protected Vector2 DesiredVelocity { get; set; }

    private readonly Collider2D[] _separationBuffer = new Collider2D[16];
    private int _enemyLayerMask;

    protected void SetAnimBool(string param, bool value)
    {
        foreach (var anim in animators)
            if (anim != null) anim.SetBool(param, value);
    }

    protected void SetAnimTrigger(string param)
    {
        foreach (var anim in animators)
            if (anim != null) anim.SetTrigger(param);
    }

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if (_isKnockedBack) return;

        if (DistanceToPlayer() <= data.attackRange && CanAttack())
        {
            PerformAttack();
            ResetAttackCooldown();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

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

            SetAnimBool("isWalking", rb.linearVelocity.sqrMagnitude > 0.01f);
            return;
        }

        if (player == null) return;

        UpdateSteering();

        Vector2 finalVelocity = DesiredVelocity + ComputeSeparationForce();
        if (finalVelocity.magnitude > data.moveSpeed * 1.4f)
            finalVelocity = finalVelocity.normalized * data.moveSpeed * 1.4f;

        rb.linearVelocity = finalVelocity;
        SetAnimBool("isWalking", rb.linearVelocity.sqrMagnitude > 0.01f);
    }

    protected abstract void UpdateSteering();

    protected virtual void PerformAttack()
    {
        SetAnimTrigger("isAttacking");
    }

    public void DealDamageToPlayer()
    {
        if (isDead) return;
        if (DistanceToPlayer() > data.attackRange * 1.5f) return;
        if (player.TryGetComponent(out PlayerHealth ph))
            ph.TakeDamage(data.damage);
    }

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
                float a = GetInstanceID() * 0.1f;
                away = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                dist = 0.001f;
            }

            float weight = 1f - Mathf.Clamp01(dist / separationRadius);
            steer += away.normalized * (weight * separationForce);
        }

        return steer;
    }

    public void TakeDamage(float amount) => TakeDamage(amount, Vector2.zero);

    public void TakeDamageSilent(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0f) Die();
    }

    public virtual void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, amount - data.defense);
        currentHealth -= finalDamage;

        if (_hitFlash == null) _hitFlash = GetComponentInChildren<HitFlash>(true);
        _hitFlash?.Flash();

        if (bloodSplatterPrefab != null)
            Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);

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

    protected float DistanceToPlayer()
        => player == null ? Mathf.Infinity : Vector2.Distance(transform.position, player.position);

    protected Vector2 DirectionToPlayer()
        => player == null ? Vector2.zero : ((Vector2)(player.position - transform.position)).normalized;

    protected void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    protected bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;
    protected void ResetAttackCooldown() => lastAttackTime = Time.time;
    public float GetHealthPercent() => currentHealth / data.maxHealth;
    public GameObject GetPlagueVFXPrefab() => plagueVFXPrefab;
    public Transform GetPlagueVFXSpawnPoint() => plagueVFXSpawnPoint != null ? plagueVFXSpawnPoint : transform;
    public Material GetPlagueMaterial() => plagueMaterial;

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

    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
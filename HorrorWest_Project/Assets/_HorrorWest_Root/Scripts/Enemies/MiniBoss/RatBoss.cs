using System.Collections;
using UnityEngine;

public class RatBoss : EnemyBase
{
    private enum BossState { Chase, TailSwipe, ChargeWindup, Charging, ChargeDerape, SummonSpit }

    [Header("Rangos de decisión")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float mediumRange = 6f;
    [SerializeField] private float farRange = 12f;

    [Header("TailSwipe")]
    [SerializeField] private float tailSwipeRadius = 2.5f;
    [SerializeField] private float tailSwipeDamage = 15f;
    [SerializeField] private float tailSwipeDuration = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Charge")]
    [SerializeField] private float chargeWindupTime = 0.8f;
    [SerializeField] private float chargeSpeed = 25f;
    [SerializeField] private float chargeDuration = 1f;
    [SerializeField] private float derapeTime = 0.5f;
    [SerializeField] private float chargeDamage = 20f;
    [SerializeField] private GameObject plagueTrailPrefab;
    [SerializeField] private float trailSpawnInterval = 0.1f;

    [Header("Spit")]
    [SerializeField] private GameObject spitProjectilePrefab;
    [SerializeField] private float spitWindupTime = 0.5f;
    [SerializeField] private int spitCount = 5;
    [SerializeField] private float spitInterval = 0.5f;
    [SerializeField] private float spitDamage = 10f;
    [SerializeField] private float spitRadius = 1f;
    [SerializeField] private float spitLeadTime = 1.2f;    // Más predictivo para ser más difícil

    [Header("Summon Rats")]
    [SerializeField] private GameObject miniRatPrefab;
    [SerializeField] private int minRatsPerSummon = 2;
    [SerializeField] private int maxRatsPerSummon = 5;
    [SerializeField] private float summonRadius = 4f;
    [SerializeField] private float summonWindupTime = 0.6f;

    [Header("Cooldowns")]
    [SerializeField] private float tailSwipeCooldown = 2f;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float comboSummonCooldown = 12f;  // Más largo para que no lo spamee

    [Header("Pesos base")]
    [Range(0f, 10f)][SerializeField] private float weightTailSwipe = 6f;  // Prioritario
    [Range(0f, 10f)][SerializeField] private float weightCharge = 3f;
    [Range(0f, 10f)][SerializeField] private float weightCombo = 1f;      // Menos frecuente

    [Header("Movimiento")]
    [SerializeField] private float _obstacleCheckCircleRadius = 0.4f;
    [SerializeField] private float _obstacleCheckDistance = 0.8f;
    [SerializeField] private float _wallScanDistance = 6f;
    [SerializeField] private LayerMask _obstacleLayerMask;
    [SerializeField] private float _rotationSpeed = 120f;
    [SerializeField] private float _avoidanceRotationSpeed = 70f;
    [SerializeField] private float _wallFollowBlend = 0.2f;

    // Estado interno
    private BossState _state = BossState.Chase;
    private bool _isExecutingAttack = false;
    private bool _isCharging = false;  // Flag para que EnemyBase no sobreescriba velocidad

    private float _tailSwipeTimer = 0f;
    private float _chargeTimer = 0f;
    private float _comboTimer = 0f;

    private Vector2 _currentMoveDir;
    private Vector2 _wallSlideDir;
    private Vector2 _wallNormal;
    private bool _isAvoiding = false;
    private Vector2 _chargeDirection;

    // Sobreescribe FixedUpdate para que durante la carga no se toque rb.linearVelocity
    // La corrutina de carga escribe directamente en rb — necesitamos que nadie lo pise
    private new void FixedUpdate()
    {
        if (_isCharging) return;  // Durante la carga la corrutina manda
        base.FixedUpdate();       // El resto del tiempo EnemyBase gestiona normalmente
    }

    protected override void UpdateSteering()
    {
        _tailSwipeTimer = Mathf.Max(0f, _tailSwipeTimer - Time.fixedDeltaTime);
        _chargeTimer = Mathf.Max(0f, _chargeTimer - Time.fixedDeltaTime);
        _comboTimer = Mathf.Max(0f, _comboTimer - Time.fixedDeltaTime);

        // Durante la carga no tocamos DesiredVelocity — la corrutina maneja rb directamente
        if (_isCharging) return;

        if (_isExecutingAttack)
        {
            DesiredVelocity = Vector2.zero;
            return;
        }

        float dist = DistanceToPlayer();
        DecideNextAction(dist);
        MoveTowardsPlayer();
    }

    private void DecideNextAction(float dist)
    {
        // Muy lejos — intenta cargar o hacer combo
        if (dist > farRange)
        {
            if (_chargeTimer <= 0f)
                StartCoroutine(ChargeAttack());
            else if (_comboTimer <= 0f)
                StartCoroutine(ComboSummonSpit());
            return;
        }

        // Melee — TailSwipe es el ataque principal, máxima prioridad
        if (dist <= meleeRange)
        {
            if (_tailSwipeTimer <= 0f)
                StartCoroutine(TailSwipeAttack());
            return;
        }

        // Rango medio — pesos dinámicos
        // TailSwipe tiene peso alto para que intente acercarse y usarlo
        float wTail = _tailSwipeTimer <= 0f ? AdjustWeight(weightTailSwipe, dist, meleeRange, mediumRange, 2f, 0.2f) : 0f;
        float wCharge = _chargeTimer <= 0f ? AdjustWeight(weightCharge, dist, mediumRange, farRange, 0.5f, 2f) : 0f;
        float wCombo = _comboTimer <= 0f ? weightCombo : 0f;

        float total = wTail + wCharge + wCombo;
        if (total <= 0f) return;

        float roll = Random.Range(0f, total);

        if (roll < wTail)
            StartCoroutine(TailSwipeAttack());
        else if (roll < wTail + wCharge)
            StartCoroutine(ChargeAttack());
        else
            StartCoroutine(ComboSummonSpit());
    }

    private float AdjustWeight(float baseW, float dist, float dMin, float dMax, float nearMult, float farMult)
    {
        float t = Mathf.InverseLerp(dMin, dMax, dist);
        return baseW * Mathf.Lerp(nearMult, farMult, t);
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) { DesiredVelocity = Vector2.zero; return; }

        Vector2 toPlayer = DirectionToPlayer();
        Vector2 ahead = _currentMoveDir == Vector2.zero ? toPlayer : _currentMoveDir.normalized;

        bool hitFront = Physics2D.CircleCast(transform.position, _obstacleCheckCircleRadius, toPlayer, _obstacleCheckDistance, _obstacleLayerMask).collider != null;
        bool hitLeft = Physics2D.CircleCast(transform.position, _obstacleCheckCircleRadius, Rotate(ahead, 45f), _obstacleCheckDistance * 0.7f, _obstacleLayerMask).collider != null;
        bool hitRight = Physics2D.CircleCast(transform.position, _obstacleCheckCircleRadius, Rotate(ahead, -45f), _obstacleCheckDistance * 0.7f, _obstacleLayerMask).collider != null;

        bool canSeePlayer = !Physics2D.Raycast(transform.position, toPlayer, DistanceToPlayer(), _obstacleLayerMask);
        if (_isAvoiding && canSeePlayer) { _isAvoiding = false; _wallSlideDir = Vector2.zero; }

        if (hitFront)
        {
            if (!_isAvoiding)
            {
                RaycastHit2D wh = Physics2D.CircleCast(transform.position, _obstacleCheckCircleRadius, toPlayer, _obstacleCheckDistance, _obstacleLayerMask);
                _wallNormal = wh.collider != null ? wh.normal : -toPlayer;

                Vector2 left = new Vector2(-_wallNormal.y, _wallNormal.x);
                Vector2 right = new Vector2(_wallNormal.y, -_wallNormal.x);

                float sL = MeasureFreeSpace(left) + Vector2.Dot(left, toPlayer) * 2f;
                float sR = MeasureFreeSpace(right) + Vector2.Dot(right, toPlayer) * 2f;
                _wallSlideDir = sL > sR ? left : right;
                _isAvoiding = true;
            }
            _currentMoveDir = Vector2.MoveTowards(_currentMoveDir,
                Vector2.Lerp(_wallSlideDir, toPlayer, _wallFollowBlend).normalized,
                _avoidanceRotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime);
        }
        else if (hitLeft && !hitRight)
            _currentMoveDir = Vector2.MoveTowards(_currentMoveDir, Rotate(ahead, -60f), _avoidanceRotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime);
        else if (hitRight && !hitLeft)
            _currentMoveDir = Vector2.MoveTowards(_currentMoveDir, Rotate(ahead, 60f), _avoidanceRotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime);
        else
        {
            _isAvoiding = false;
            _wallSlideDir = Vector2.zero;
            _currentMoveDir = Vector2.MoveTowards(_currentMoveDir, toPlayer, _rotationSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime);
        }

        FaceDirection(_currentMoveDir);
        DesiredVelocity = _currentMoveDir * data.moveSpeed;
    }

    // ── TailSwipe ─────────────────────────────────────────────────────────────
    private IEnumerator TailSwipeAttack()
    {
        _isExecutingAttack = true;
        _state = BossState.TailSwipe;

        yield return new WaitForSeconds(0.25f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, tailSwipeRadius, playerLayer);
        foreach (Collider2D hit in hits)
            hit.GetComponent<PlayerHealth>()?.TakeDamage(tailSwipeDamage);

        yield return new WaitForSeconds(tailSwipeDuration);

        _tailSwipeTimer = tailSwipeCooldown;
        _isExecutingAttack = false;
        _state = BossState.Chase;
    }

    // ── Charge ────────────────────────────────────────────────────────────────
    private IEnumerator ChargeAttack()
    {
        _isExecutingAttack = true;
        _state = BossState.ChargeWindup;

        // Windup — quieto mirando al player
        float windupElapsed = 0f;
        while (windupElapsed < chargeWindupTime)
        {
            FaceDirection(DirectionToPlayer());
            windupElapsed += Time.deltaTime;
            yield return null;
        }

        // Fija dirección al final del windup
        _chargeDirection = DirectionToPlayer();
        _state = BossState.Charging;
        _isCharging = true;

        float elapsed = 0f;
        float trailTimer = 0f;

        while (elapsed < chargeDuration)
        {
            rb.linearVelocity = _chargeDirection * chargeSpeed;

            trailTimer += Time.deltaTime;
            if (trailTimer >= trailSpawnInterval && plagueTrailPrefab != null)
            {
                Instantiate(plagueTrailPrefab, transform.position, Quaternion.identity);
                trailTimer = 0f;
            }

            if (DistanceToPlayer() <= data.attackRange)
                player?.GetComponent<PlayerHealth>()?.TakeDamage(chargeDamage);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Derrape
        _state = BossState.ChargeDerape;
        float derapeElapsed = 0f;
        Vector2 startVel = _chargeDirection * chargeSpeed;

        while (derapeElapsed < derapeTime)
        {
            rb.linearVelocity = Vector2.Lerp(startVel, Vector2.zero, derapeElapsed / derapeTime);
            derapeElapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        _isCharging = false;
        _chargeTimer = chargeCooldown;
        _isExecutingAttack = false;
        _state = BossState.Chase;
    }

    // ── Combo Summon + Spit ───────────────────────────────────────────────────
    private IEnumerator ComboSummonSpit()
    {
        _isExecutingAttack = true;
        _state = BossState.SummonSpit;

        yield return new WaitForSeconds(summonWindupTime);

        int ratCount = Random.Range(minRatsPerSummon, maxRatsPerSummon + 1);
        for (int i = 0; i < ratCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle.normalized * summonRadius;
            if (miniRatPrefab != null)
                Instantiate(miniRatPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.4f);
        yield return new WaitForSeconds(spitWindupTime);

        for (int i = 0; i < spitCount; i++)
        {
            if (player == null) break;

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            Vector2 playerVel = playerRb != null ? playerRb.linearVelocity : Vector2.zero;
            Vector2 predictedPos = (Vector2)player.position + playerVel * spitLeadTime;
            Vector2 direction = (predictedPos - (Vector2)transform.position).normalized;

            if (spitProjectilePrefab != null)
            {
                GameObject spit = Instantiate(spitProjectilePrefab, transform.position, Quaternion.identity);
                spit.GetComponent<SpitProjectile>()?.Init(direction, spitDamage, spitRadius);
            }

            if (i < spitCount - 1)
                yield return new WaitForSeconds(spitInterval);
        }

        yield return new WaitForSeconds(0.3f);

        _comboTimer = comboSummonCooldown;
        _isExecutingAttack = false;
        _state = BossState.Chase;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private float MeasureFreeSpace(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _wallScanDistance, _obstacleLayerMask);
        return hit.collider != null ? hit.distance : _wallScanDistance;
    }

    private Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mediumRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, farRange);
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, tailSwipeRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }
}
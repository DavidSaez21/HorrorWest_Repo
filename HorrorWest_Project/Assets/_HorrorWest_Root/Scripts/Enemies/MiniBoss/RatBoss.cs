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

    [Header("Summon Rats")]
    [SerializeField] private GameObject miniRatPrefab;
    [SerializeField] private int minRatsPerSummon = 2;
    [SerializeField] private int maxRatsPerSummon = 5;
    [SerializeField] private float summonRadius = 4f;
    [SerializeField] private float summonWindupTime = 0.6f;

    [Header("Cooldowns")]
    [SerializeField] private float tailSwipeCooldown = 2f;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float comboSummonCooldown = 12f;

    [Header("Pesos base")]
    [Range(0f, 10f)][SerializeField] private float weightCharge = 3f;
    [Range(0f, 10f)][SerializeField] private float weightCombo = 1f;

    [Header("Movimiento (Context Steering)")]
    [Range(8, 32)][SerializeField] private int _contextSlots = 16;
    [Range(0.3f, 2f)][SerializeField] private float _probeLength = 0.9f;
    [Range(0f, 1f)][SerializeField] private float _dangerThreshold = 0.2f;
    [Range(3f, 20f)][SerializeField] private float _steerSmoothing = 10f;
    [SerializeField] private LayerMask _obstacleLayerMask;

    // Estado interno
    private BossState _state = BossState.Chase;
    private bool _isExecutingAttack = false;
    private bool _isCharging = false;

    private float _tailSwipeTimer = 0f;
    private float _chargeTimer = 0f;
    private float _comboTimer = 0f;

    private Vector2 _currentHeading;
    private Vector2[] _slotDirections;
    private float[] _interest;
    private float[] _danger;
    private Vector2 _chargeDirection;

    protected override void Awake()
    {
        base.Awake();
        AllocateContextArrays();
    }

    protected override void FixedUpdate()
    {
        if (_isCharging) return;
        base.FixedUpdate();
    }

    protected override void UpdateSteering()
    {
        _tailSwipeTimer = Mathf.Max(0f, _tailSwipeTimer - Time.fixedDeltaTime);
        _chargeTimer = Mathf.Max(0f, _chargeTimer - Time.fixedDeltaTime);
        _comboTimer = Mathf.Max(0f, _comboTimer - Time.fixedDeltaTime);

        if (_isCharging) return;

        float dist = DistanceToPlayer();

        // En rango melee — para y hace TailSwipe
        if (dist <= meleeRange)
        {
            DesiredVelocity = Vector2.zero;
            if (!_isExecutingAttack && _tailSwipeTimer <= 0f)
                StartCoroutine(TailSwipeAttack());
            return;
        }

        // Fuera de melee — decide ataques y SIEMPRE se mueve
        if (!_isExecutingAttack)
            DecideNextAction(dist);

        MoveTowardsPlayer();
    }

    private void DecideNextAction(float dist)
    {
        // Muy lejos — carga o combo
        if (dist > farRange)
        {
            if (_chargeTimer <= 0f)
                StartCoroutine(ChargeAttack());
            else if (_comboTimer <= 0f)
                StartCoroutine(ComboSummonSpit());
            return;
        }

        // Rango medio — pesos dinámicos entre charge y combo
        float wCharge = _chargeTimer <= 0f ? AdjustWeight(weightCharge, dist, mediumRange, farRange, 0.5f, 2f) : 0f;
        float wCombo = _comboTimer <= 0f ? weightCombo : 0f;

        float total = wCharge + wCombo;
        if (total <= 0f) return;

        float roll = Random.Range(0f, total);
        if (roll < wCharge)
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

        Vector2 steerDir = ApplyContextSteering(DirectionToPlayer());
        _currentHeading = Vector2.Lerp(_currentHeading, steerDir, _steerSmoothing * Time.fixedDeltaTime).normalized;
        FaceDirection(_currentHeading);
        DesiredVelocity = _currentHeading * data.moveSpeed;
    }

    private Vector2 ApplyContextSteering(Vector2 desiredDir)
    {
        for (int i = 0; i < _contextSlots; i++)
            _interest[i] = Mathf.Max(0f, Vector2.Dot(_slotDirections[i], desiredDir));

        for (int i = 0; i < _contextSlots; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _slotDirections[i], _probeLength, _obstacleLayerMask);
            _danger[i] = hit.collider != null ? 1f - Mathf.Clamp01(hit.distance / _probeLength) : 0f;
        }

        Vector2 best = desiredDir;
        float bestScore = -1f;
        bool anyValid = false;

        for (int i = 0; i < _contextSlots; i++)
        {
            if (_danger[i] > _dangerThreshold) continue;
            if (_interest[i] > bestScore) { bestScore = _interest[i]; best = _slotDirections[i]; anyValid = true; }
        }

        if (!anyValid)
        {
            float lowestDanger = float.MaxValue;
            for (int i = 0; i < _contextSlots; i++)
                if (_danger[i] < lowestDanger) { lowestDanger = _danger[i]; best = _slotDirections[i]; }
        }

        return best;
    }

    private void AllocateContextArrays()
    {
        _slotDirections = new Vector2[_contextSlots];
        _interest = new float[_contextSlots];
        _danger = new float[_contextSlots];
        float step = 360f / _contextSlots;
        for (int i = 0; i < _contextSlots; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            _slotDirections[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
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

        float windupElapsed = 0f;
        while (windupElapsed < chargeWindupTime)
        {
            FaceDirection(DirectionToPlayer());
            windupElapsed += Time.deltaTime;
            yield return null;
        }

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

            if (DistanceToPlayer() <= meleeRange)
                player?.GetComponent<PlayerHealth>()?.TakeDamage(chargeDamage * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

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
            Vector2 spawnPos = player != null
                ? (Vector2)player.position + Random.insideUnitCircle.normalized * summonRadius
                : (Vector2)transform.position + Random.insideUnitCircle.normalized * summonRadius;
            if (miniRatPrefab != null)
                Instantiate(miniRatPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.4f);
        yield return new WaitForSeconds(spitWindupTime);

        for (int i = 0; i < spitCount; i++)
        {
            if (player == null) break;
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;

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
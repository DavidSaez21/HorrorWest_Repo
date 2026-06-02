using UnityEngine;

/// <summary>
/// Main script for the Church Boss — La Iglesia.
///
/// Inherits from EnemyBase which provides:
///   • Health, damage, knockback, death, drops, XP reward
///   • IDamageable interface (TakeDamage)
///   • Separation steering (unused here — boss is static)
///
/// This script's responsibilities:
///   1. Override UpdateSteering() to do nothing (boss never moves).
///   2. Initialise all attack components with data from ChurchBossData.
///   3. Tick BossActionQueue every Update.
///   4. Override OnDamageReceived for hit feedback (flash).
///   5. Override OnDeath to stop all attacks cleanly.
///
/// ── HOW TO SET UP IN UNITY ────────────────────────────────────────────────────
///   1. Place the boss sprite at the altar position in your scene.
///   2. Add this component. It requires a Rigidbody2D (from EnemyBase).
///   3. Assign a ChurchBossData ScriptableObject to the "Data" field (inherited).
///   4. Add child GameObjects / components for each attack and assign them below.
///   5. Set the Rigidbody2D to Kinematic so the boss cannot be moved by physics.
///   6. Tag the boss GameObject as "Enemy" (or whatever your layer setup expects).
/// </summary>
[RequireComponent(typeof(BossActionQueue))]
public class ChurchBoss : EnemyBase
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("── Church Boss Components ───────────────────")]
    [SerializeField] private BossEyeTracker eyeTracker;
    [SerializeField] private TongueAttack tongueAttack;
    [SerializeField] private TentacleAttack tentacleAttack;
    [SerializeField] private GroundMouthAttack groundMouthAttack;
    [SerializeField] private MinionSpawner minionSpawner;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer[] spriteRenderers; // all renderers to flash
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;

    // ── Cached components ─────────────────────────────────────────────────────

    private BossActionQueue _actionQueue;
    private ChurchBossData _bossData;

    // Flash coroutine state
    private Coroutine _flashCoroutine;

    // ── EnemyBase overrides ───────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        _actionQueue = GetComponent<BossActionQueue>();

        // Make the Rigidbody2D kinematic — boss never moves via physics
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected override void Start()
    {
        base.Start();

        // Cast EnemyData to ChurchBossData for extended stats
        _bossData = data as ChurchBossData;
        if (_bossData == null)
        {
            Debug.LogError("[ChurchBoss] 'data' must be a ChurchBossData ScriptableObject!");
            return;
        }

        InitialiseAttacks();

        _actionQueue.Initialise(
            player,
            tongueAttack,
            tentacleAttack,
            groundMouthAttack,
            minionSpawner,
            eyeTracker);

        eyeTracker?.Initialise(player);
    }

    protected override void Update()
    {
        if (isDead || player == null) return;
        // EnemyBase.Update() handles attack range + PerformAttack() calls.
        // We skip that entirely for the boss — BossActionQueue manages attacks.
        // So we do NOT call base.Update().

        _actionQueue.Tick();
    }

    /// <summary>
    /// Boss is static — override UpdateSteering and set DesiredVelocity to zero.
    /// </summary>
    public override void TakeDamage(float amount, Vector2 hitDirection)
    {
        base.TakeDamage(amount, Vector2.zero);
    }

    protected override void UpdateSteering()
    {
        DesiredVelocity = Vector2.zero;
    }

    /// <summary>
    /// Boss attacks are driven by BossActionQueue, not by EnemyBase's range check.
    /// This override is intentionally empty.
    /// </summary>
    protected override void PerformAttack() { }

    protected override void OnDamageReceived(float damage)
    {
        // Flash all sprite renderers white
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    protected override void OnDeath()
    {
        _actionQueue.StopAll();
        // Add death animation trigger, sfx, scene transition here
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void InitialiseAttacks()
    {
        if (tongueAttack != null)
        {
            tongueAttack.windupDuration = _bossData.tongueWindupDuration;
            tongueAttack.lungeDuration = _bossData.tongueLungeDuration;
            tongueAttack.lingerDuration = _bossData.tongueLingerDuration; // Note: property name matches field
            tongueAttack.retractDuration = _bossData.tongueRetractDuration;
            tongueAttack.damage = _bossData.tongueDamage;
        }

        if (tentacleAttack != null)
        {
            tentacleAttack.telegraphDuration = _bossData.tentacleTelegraphDuration;
            tentacleAttack.sweepDuration = _bossData.tentacleSweepDuration;
            tentacleAttack.retractDuration = _bossData.tentacleRetractDuration;
            tentacleAttack.damage = _bossData.tentacleDamage;
        }

        if (groundMouthAttack != null)
        {
            groundMouthAttack.crackDuration = _bossData.mouthCrackDuration;
            groundMouthAttack.openDuration = _bossData.mouthOpenDuration;
            groundMouthAttack.biteDuration = _bossData.mouthBiteDuration;
            groundMouthAttack.openTickDPS = _bossData.mouthOpenDPS;
            groundMouthAttack.biteDamage = _bossData.mouthBiteDamage;
        }

        if (minionSpawner != null)
        {
            minionSpawner.staggerOverride = _bossData.minionSpawnStagger;
        }
    }

    // ── Hit flash ─────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator FlashRoutine()
    {
        SetFlashColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetFlashColor(Color.white); // restore normal tint
        _flashCoroutine = null;
    }

    private void SetFlashColor(Color color)
    {
        if (spriteRenderers == null) return;
        foreach (var sr in spriteRenderers)
            if (sr != null) sr.color = color;
    }

    // ── Debug gizmos ──────────────────────────────────────────────────────────

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        // Draw a rough footprint of the boss
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 2f, 0f));
    }
}
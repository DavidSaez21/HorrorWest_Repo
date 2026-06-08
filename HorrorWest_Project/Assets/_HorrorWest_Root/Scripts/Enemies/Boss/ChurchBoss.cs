using UnityEngine;

/// <summary>
/// Script principal del Church Boss — La Iglesia.
/// Hereda de EnemyBase (vida, daño, muerte, drops, XP).
/// Toda la configuración de ataques se hace desde el Inspector.
/// </summary>
[RequireComponent(typeof(BossActionQueue))]
public class ChurchBoss : EnemyBase
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("── Componentes de ataque ───────────────────")]
    [SerializeField] private BossEyeTracker eyeTracker;
    [SerializeField] private TongueAttack tongueAttack;
    [SerializeField] private TentacleAttack tentacleAttack;
    [SerializeField] private GroundMouthAttack groundMouthAttack;
    [SerializeField] private MinionSpawner minionSpawner;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;

    // ── Cached ────────────────────────────────────────────────────────────────

    private BossActionQueue _actionQueue;
    private ChurchBossData _bossData;
    private Coroutine _flashCoroutine;

    // ── EnemyBase overrides ───────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        _actionQueue = GetComponent<BossActionQueue>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected override void Start()
    {
        base.Start();

        _bossData = data as ChurchBossData;
        if (_bossData == null)
        {
            Debug.LogError("[ChurchBoss] El campo 'Data' debe ser un ChurchBossData ScriptableObject.");
            return;
        }

        InitialiseAttacks();

        _actionQueue.Initialise(
            player, tongueAttack, tentacleAttack,
            groundMouthAttack, minionSpawner, eyeTracker);

        eyeTracker?.Initialise(player);
    }

    protected override void Update()
    {
        if (isDead || player == null) return;
        _actionQueue.Tick();
    }

    public override void TakeDamage(float amount, Vector2 hitDirection)
    {
        // Sin knockback — el boss es estático
        base.TakeDamage(amount, Vector2.zero);
    }

    protected override void UpdateSteering()
    {
        DesiredVelocity = Vector2.zero;
    }

    protected override void PerformAttack() { }

    protected override void OnDamageReceived(float damage)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    protected override void OnDeath()
    {
        _actionQueue.StopAll();
    }

    // ── Inicialización de ataques ─────────────────────────────────────────────

    private void InitialiseAttacks()
    {
        if (tongueAttack != null)
        {
            tongueAttack.biteDamage = _bossData.tongueBiteDamage;
            tongueAttack.sweepDamage = _bossData.tongueSweepDamage;
            tongueAttack.biteDuration = _bossData.tongueBiteDuration;
            tongueAttack.sweepDuration = _bossData.tongueSweepDuration;
        }

        if (tentacleAttack != null)
        {
            tentacleAttack.damage = _bossData.tentacleDamage;
            tentacleAttack.emergeDuration = _bossData.tentacleEmergeDuration;
            tentacleAttack.waitAtTargetDuration = _bossData.tentacleWaitAtTargetDuration;
            tentacleAttack.attackAnimDuration = _bossData.tentacleAttackAnimDuration;
            tentacleAttack.retractDuration = _bossData.tentacleRetractDuration;
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
            minionSpawner.staggerOverride = _bossData.minionSpawnStagger;
    }

    // ── Hit flash ─────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator FlashRoutine()
    {
        SetFlashColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetFlashColor(Color.white);
        _flashCoroutine = null;
    }

    private void SetFlashColor(Color color)
    {
        if (spriteRenderers == null) return;
        foreach (var sr in spriteRenderers)
            if (sr != null) sr.color = color;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 2f, 0f));
    }
}
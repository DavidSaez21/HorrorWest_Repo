using UnityEngine;

[RequireComponent(typeof(BossActionQueue))]
public class ChurchBoss : EnemyBase
{
    [Header("── Componentes de ataque ───────────────────")]
    [SerializeField] private BossEyeTracker eyeTracker;
    [SerializeField] private TongueAttack tongueAttack;
    [SerializeField] private TentacleAttack tentacleLeft;
    [SerializeField] private TentacleAttack tentacleRight;
    [SerializeField] private GroundMouthAttack groundMouthAttack;
    [SerializeField] private MinionSpawner minionSpawner;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private Color flashColor = Color.white;

    private BossActionQueue _actionQueue;
    private ChurchBossData _bossData;
    private Coroutine _flashCoroutine;

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
            player, tongueAttack, tentacleLeft, tentacleRight,
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
        base.TakeDamage(amount, Vector2.zero);
    }

    protected override void UpdateSteering() => DesiredVelocity = Vector2.zero;
    protected override void PerformAttack() { }

    protected override void OnDamageReceived(float damage)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    protected override void OnDeath()
    {
        _actionQueue.StopAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("SCN_Victory");
    }

    private void InitialiseAttacks()
    {
        // Solo daños — las duraciones se configuran en el Inspector de cada componente
        if (tongueAttack != null)
        {
            tongueAttack.biteDamage = _bossData.tongueBiteDamage;
            tongueAttack.sweepDamage = _bossData.tongueSweepDamage;
        }

        if (tentacleLeft != null) tentacleLeft.damage = _bossData.tentacleDamage;
        if (tentacleRight != null) tentacleRight.damage = _bossData.tentacleDamage;

        if (groundMouthAttack != null)
        {
            groundMouthAttack.biteDamage = _bossData.mouthBiteDamage;
            groundMouthAttack.openTickDPS = _bossData.mouthOpenDPS;
        }

        if (minionSpawner != null)
            minionSpawner.staggerOverride = _bossData.minionSpawnStagger;
    }

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
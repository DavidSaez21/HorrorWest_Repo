using UnityEngine;

// CBS + flanqueo lateral. El más inteligente de los tres.
// Inspector: moveSpeed ~3, maxHealth ~60, damage ~12
public class EnemyFlanker : EnemyBase
{
    [Header("Flanking")]
    public float flankStartRange = 5f;
    [Range(0f, 1f)]
    public float flankWeight = 0.55f;
    public float flankSwitchInterval = 3f;

    private EnemyCBS cbs;
    private float flankSign = 1f;
    private float flankTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        cbs = GetComponent<EnemyCBS>();
    }

    protected override void UpdateMovement(float distToPlayer)
    {
        flankTimer += Time.deltaTime;
        if (flankTimer >= flankSwitchInterval)
        {
            flankSign = -flankSign;
            flankTimer = 0f;
        }

        Vector2 toPlayer = DirectionToPlayer();
        Vector2 flankPerp = new Vector2(-toPlayer.y, toPlayer.x) * flankSign;

        Vector2 desired = distToPlayer < flankStartRange
            ? Vector2.Lerp(toPlayer, flankPerp, flankWeight).normalized
            : toPlayer;

        Vector2 dir = cbs != null ? cbs.GetBestDirection(desired) : desired;

        FaceDirection(dir);

        if (distToPlayer > data.attackRange)
            Move(dir);
        else
            rb.linearVelocity = Vector2.zero;
    }
}
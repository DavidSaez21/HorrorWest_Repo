using UnityEngine;

// Lento, con armadura y knockback. CBS con poco peso lateral
// para que vaya casi siempre recto hacia el jugador.
// Inspector: moveSpeed ~1.2, maxHealth ~200, damage ~20
public class EnemyTank : EnemyBase
{
    [Header("Tank")]
    [Range(0f, 0.8f)]
    public float damageReduction = 0.3f;
    public float playerKnockbackForce = 4f;

    private EnemyCBS cbs;

    protected override void Awake()
    {
        base.Awake();
        cbs = GetComponent<EnemyCBS>();
    }

    protected override void UpdateMovement(float distToPlayer)
    {
        Vector2 desired = DirectionToPlayer();
        // Tank: CBS con deseo muy directo, apenas se desvía
        Vector2 dir = cbs != null ? cbs.GetBestDirection(desired) : desired;

        FaceDirection(dir);

        if (distToPlayer > data.attackRange)
            Move(dir);
        else
            rb.linearVelocity = Vector2.zero;
    }

    public override void TakeDamage(float amount, Vector2 hitDirection)
    {
        base.TakeDamage(amount * (1f - damageReduction), hitDirection);
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.AddForce(DirectionToPlayer() * playerKnockbackForce, ForceMode2D.Impulse);
    }
}
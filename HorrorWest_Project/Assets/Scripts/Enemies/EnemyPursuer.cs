using UnityEngine;

public class EnemyPursuer : EnemyBase
{
    private EnemyCBS cbs;

    protected override void Awake()
    {
        base.Awake();
        cbs = GetComponent<EnemyCBS>();
    }

    protected override void UpdateMovement(float distToPlayer)
    {
        Vector2 desired = DirectionToPlayer();
        Vector2 dir = cbs != null ? cbs.GetBestDirection(desired) : desired;
        FaceDirection(dir);

        if (distToPlayer > data.attackRange)
            Move(dir);
        else
            rb.linearVelocity = Vector2.zero;
    }
}
using UnityEngine;

[RequireComponent(typeof(ContextSteering))]
public class MeleeEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (isDead || player == null) return;

        if (DistanceToPlayer() <= data.attackRange)
            TryMeleeAttack();
    }

    private void TryMeleeAttack()
    {
        if (!CanAttack()) return;
        ResetAttackCooldown();

        if (player.TryGetComponent(out PlayerHealth health))
            health.TakeDamage(data.damage);
    }

    protected override void OnDamageReceived(float damage) { }
    protected override void OnDeath() { }
}
using UnityEngine;

/// <summary>
/// Enemigo cuerpo a cuerpo — usa ContextSteering para el movimiento
/// y hereda vida, drops y XP de EnemyBase.
/// </summary>
[RequireComponent(typeof(ContextSteering))]
public class MeleeEnemy : EnemyBase
{
    private ContextSteering steering;

    protected override void Start()
    {
        base.Start();
        steering = GetComponent<ContextSteering>();
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
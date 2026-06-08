using UnityEngine;

public class EnemyAttackEvent : MonoBehaviour
{
    private EnemyBase _enemyBase;

    private void Awake()
    {
        // Busca el EnemyBase en el padre raíz
        _enemyBase = GetComponentInParent<EnemyBase>();
    }

    // Llamar desde el Animation Event en el frame del golpe
    public void DealDamageToPlayer()
    {
        _enemyBase?.DealDamageToPlayer();
    }
}
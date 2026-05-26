using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("References")]
    [SerializeField] protected GameObject[] coinPrefabs;

    protected float currentHealth;
    protected float lastAttackTime;
    protected Transform player;
    protected bool isDead = false;

    protected virtual void Start()
    {
        currentHealth = data.maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, amount - data.defense);
        currentHealth -= finalDamage;

        OnDamageReceived(finalDamage);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;

        ExperienceManager.Instance?.AddXP(data.xpReward);
        TryDropCoin();
        TryInstantReload();
        OnDeath();

        Destroy(gameObject);
    }

    private void TryInstantReload()
    {
        if (PlayerStats.Instance == null) return;
        if (!PlayerStats.Instance.hasReloadOnKill) return;

        // Comprueba probabilidad
        if (Random.value > PlayerStats.Instance.GetReloadOnKillChance()) return;

        // Recarga el arma actual instantáneamente
        PlayerShoot playerShoot = FindFirstObjectByType<PlayerShoot>();
        if (playerShoot == null) return;

        WeaponBase weapon = playerShoot.GetCurrentWeapon();
        if (weapon == null) return;

        if (weapon is Revolver revolver) revolver.InstantReload();
        else if (weapon is Shotgun shotgun) shotgun.InstantReload();
        else if (weapon is Rifle rifle) rifle.InstantReload();
    }

    protected virtual void OnDamageReceived(float damage) { }
    protected virtual void OnDeath() { }

    protected float DistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 DirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)player.position - (Vector2)transform.position).normalized;
    }

    protected bool CanAttack() => Time.time >= lastAttackTime + data.attackCooldown;
    protected void ResetAttackCooldown() => lastAttackTime = Time.time;

    private void TryDropCoin()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0) return;
        if (Random.value > data.dropChance) return;

        float total = data.dropChanceCoinOne + data.dropChanceCoinFive + data.dropChanceCoinTen;
        float roll = Random.Range(0f, total);

        int index;
        if (roll < data.dropChanceCoinOne) index = 0;
        else if (roll < data.dropChanceCoinOne + data.dropChanceCoinFive) index = 1;
        else index = 2;

        index = Mathf.Clamp(index, 0, coinPrefabs.Length - 1);

        if (coinPrefabs[index] != null)
            Instantiate(coinPrefabs[index], transform.position, Quaternion.identity);
    }
}
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseBulletSpeed = 1f;
    [SerializeField] private float baseFireRate = 1f;
    [SerializeField] private float baseReloadSpeed = 1f;
    [SerializeField] private float baseLuck = 1f;

    [Header("Critical Stats")]
    [SerializeField] private float baseCritChance = 0f;         // 0% al inicio
    [SerializeField] private float baseCritMultiplier = 1.25f;  // 25% más de daño

    [Header("References")]
    [SerializeField] private PlayerShoot playerShoot;

    // Multiplicadores in-run
    private float damageMultiplier = 1f;
    private float moveSpeedMultiplier = 1f;
    private float maxHealthBonus = 0f;
    private float bulletSpeedMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private float reloadSpeedMultiplier = 1f;
    private float luckMultiplier = 1f;

    // Crítico — se mejora desde la tienda permanente
    private float critChanceBonus = 0f;         // Bonus añadido desde la tienda
    private float critMultiplierBonus = 0f;     // Bonus al multiplicador desde la tienda

    #region Debug
    [Header("Debug - Current Stats (Read Only)")]
    [SerializeField] private float _currentDamage;
    [SerializeField] private float _currentMoveSpeed;
    [SerializeField] private float _currentMaxHealth;
    [SerializeField] private float _currentBulletSpeed;
    [SerializeField] private float _currentFireRate;
    [SerializeField] private float _currentReloadSpeed;
    [SerializeField] private float _currentLuck;
    [SerializeField] private float _currentCritChance;
    [SerializeField] private float _currentCritMultiplier;
    #endregion

    public bool hasPiercingBullets { get; private set; }
    public bool hasCoinMagnet { get; private set; }
    public bool hasReloadOnKill { get; private set; }
    public bool hasDoubleShot { get; private set; }
    public bool hasPlagueBullets { get; private set; }
    public bool hasPlagueChain { get; private set; }
    public bool hasExplosiveBullets { get; private set; }
    public bool hasDualWield { get; private set; }
    public bool hasTotalPlague { get; private set; }
    public bool hasSpecter { get; private set; }
    public bool hasLastBullet { get; private set; }
    public bool hasGunfightersCurse { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        UpdateDebugStats();
    }

    // Calcula si un disparo es crítico y devuelve el daño final
    public float CalculateDamage(float baseDmg)
    {
        float critChance = GetCritChance();

        // Bonus de apuntado
        if (PlayerAim.Instance != null && PlayerAim.Instance.IsAiming)
            critChance *= PlayerAim.Instance.GetCritMultiplier();

        bool isCrit = Random.value < critChance;
        float finalDamage = baseDmg * GetDamage();

        if (isCrit)
            finalDamage *= GetCritMultiplier();

        return finalDamage;
    }

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.DamageUp: damageMultiplier += upgrade.value; break;
            case UpgradeType.MoveSpeedUp: moveSpeedMultiplier += upgrade.value; break;
            case UpgradeType.MaxHealthUp: maxHealthBonus += upgrade.value; break;
            case UpgradeType.BulletSpeedUp: bulletSpeedMultiplier += upgrade.value; break;
            case UpgradeType.FireRateUp: fireRateMultiplier += upgrade.value; break;
            case UpgradeType.ReloadSpeedUp: reloadSpeedMultiplier += upgrade.value; break;

            case UpgradeType.PiercingBullets: hasPiercingBullets = true; break;
            case UpgradeType.CoinMagnet: hasCoinMagnet = true; break;
            case UpgradeType.ReloadOnKill: hasReloadOnKill = true; break;
            case UpgradeType.DoubleShot: hasDoubleShot = true; break;
            case UpgradeType.PlagueBullets: hasPlagueBullets = true; break;
            case UpgradeType.PlagueChain: hasPlagueChain = true; break;
            case UpgradeType.ExplosiveBullets: hasExplosiveBullets = true; break;
            case UpgradeType.GunfightersCurse: hasGunfightersCurse = true; break;
            case UpgradeType.Specter: hasSpecter = true; break;
            case UpgradeType.LastBullet: hasLastBullet = true; break;
            case UpgradeType.TotalPlague: hasTotalPlague = true; break;

            case UpgradeType.DualWield:
                hasDualWield = true;
                if (playerShoot != null)
                    playerShoot.ActivateDualWield();
                break;
        }

        UpdateDebugStats();
    }

    // Llamado desde la tienda permanente
    public void AddCritChance(float amount)
    {
        critChanceBonus += amount;
        UpdateDebugStats();
    }

    public void AddCritMultiplier(float amount)
    {
        critMultiplierBonus += amount;
        UpdateDebugStats();
    }

    private void UpdateDebugStats()
    {
        _currentDamage = GetDamage();
        _currentMoveSpeed = GetMoveSpeed();
        _currentMaxHealth = GetMaxHealth();
        _currentBulletSpeed = GetBulletSpeed();
        _currentFireRate = GetFireRate();
        _currentReloadSpeed = GetReloadSpeed();
        _currentLuck = GetLuckMultiplier();
        _currentCritChance = GetCritChance();
        _currentCritMultiplier = GetCritMultiplier();
    }

    public void ResetStats()
    {
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        maxHealthBonus = 0f;
        bulletSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
        reloadSpeedMultiplier = 1f;
        luckMultiplier = baseLuck;

        hasPiercingBullets = false;
        hasCoinMagnet = false;
        hasReloadOnKill = false;
        hasDoubleShot = false;
        hasPlagueBullets = false;
        hasPlagueChain = false;
        hasExplosiveBullets = false;
        hasDualWield = false;
        hasTotalPlague = false;
        hasSpecter = false;
        hasLastBullet = false;
        hasGunfightersCurse = false;

        UpdateDebugStats();
    }

    public float GetDamage() => baseDamage * damageMultiplier;
    public float GetMoveSpeed() => baseMoveSpeed * moveSpeedMultiplier;
    public float GetMaxHealth() => baseMaxHealth + maxHealthBonus;
    public float GetBulletSpeed() => baseBulletSpeed * bulletSpeedMultiplier;
    public float GetFireRate() => baseFireRate * fireRateMultiplier;
    public float GetReloadSpeed() => baseReloadSpeed * reloadSpeedMultiplier;
    public float GetLuckMultiplier() => luckMultiplier;
    public float GetCritChance() => baseCritChance + critChanceBonus;
    public float GetCritMultiplier() => baseCritMultiplier + critMultiplierBonus;
}
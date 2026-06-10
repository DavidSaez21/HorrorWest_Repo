using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// PlayerStats — gestiona todas las estadísticas y mejoras del jugador.
// Acceso externo: PlayerManager.Instance.Stats
// ─────────────────────────────────────────────────────────────────────────────
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseBulletSpeed = 1f;
    [SerializeField] private float baseFireRate = 1f;
    [SerializeField] private float baseReloadSpeed = 1f;
    [SerializeField] private float baseLuck = 1f;

    [Header("Critical Stats")]
    [SerializeField] private float baseCritChance = 0f;
    [SerializeField] private float baseCritMultiplier = 1.25f;

    [Header("Special Settings")]
    [SerializeField] private float executeThreshold = 0.05f;
    [SerializeField] private float gunfightersCurseMaxBonus = 1f;

    [Header("References")]
    [SerializeField] private PlayerShoot playerShoot;

    // ── Bonuses permanentes (de la tienda) ────────────────────────────────────
    // Se aplican en ResetStats() desde PermanentUpgradeManager.
    // Separados de los multiplicadores in-run para no mezclarse.
    private float _permDamageBonus = 0f;
    private float _permReloadSpeedBonus = 0f;
    private float _permMaxHealthBonus = 0f;
    private float _permArmor = 0f;
    private float _permLuckBonus = 0f;
    private float _permXPBonus = 0f;
    private float _permLootBonus = 0f;

    // ── Multiplicadores in-run ────────────────────────────────────────────────
    private float damageMultiplier = 1f;
    private float moveSpeedMultiplier = 1f;
    private float maxHealthBonus = 0f;
    private float bulletSpeedMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private float reloadSpeedMultiplier = 1f;
    private float luckMultiplier = 1f;
    private float reloadOnKillChance = 0f;
    private float bulletSizeBonus = 0f;
    private float critChanceBonus = 0f;
    private float critMultiplierBonus = 0f;

    // ── Flags de mejoras únicas ───────────────────────────────────────────────
    public bool hasPiercingBullets { get; private set; }
    public bool hasCoinMagnet { get; private set; }
    public bool hasReloadOnKill { get; private set; }
    public bool hasExecute { get; private set; }
    public bool hasDoubleShot { get; private set; }
    public bool hasPlagueBullets { get; private set; }
    public bool hasPlagueChain { get; private set; }
    public bool hasExplosiveBullets { get; private set; }
    public bool hasDualWield { get; private set; }
    public bool hasTotalPlague { get; private set; }
    public bool hasSpecter { get; private set; }
    public bool hasLastBullet { get; private set; }
    public bool hasGunfightersCurse { get; private set; }
    public bool hasInfiniteAmmo { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        luckMultiplier = baseLuck;
        UpdateDebugStats();
    }

    private void Update()
    {
        if (hasGunfightersCurse)
            UpdateDebugStats();
    }

    // ── Cálculo de daño ───────────────────────────────────────────────────────
    public float CalculateDamage(float baseDmg)
    {
        float critChance = GetCritChance();
        if (PlayerAiming.Instance != null && PlayerAiming.Instance.IsAiming)
            critChance *= PlayerAiming.Instance.GetCritMultiplier();

        float finalDamage = baseDmg * GetDamage();
        if (Random.value < critChance)
            finalDamage *= GetCritMultiplier();

        if (hasGunfightersCurse)
        {
            PlayerHealth ph = PlayerManager.Instance?.Health;
            if (ph != null)
            {
                float healthPercent = ph.GetCurrentHealth() / ph.GetMaxHealth();
                float curseMultiplier = 1f + (1f - healthPercent) * gunfightersCurseMaxBonus;
                finalDamage *= curseMultiplier;
            }
        }

        return finalDamage;
    }

    // ── Mejoras in-run ────────────────────────────────────────────────────────
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.DamageUp: damageMultiplier += upgrade.value; break;
            case UpgradeType.MoveSpeedUp: moveSpeedMultiplier += upgrade.value; break;
            case UpgradeType.MaxHealthUp: maxHealthBonus += upgrade.value; break;
            case UpgradeType.BulletSpeedUp: bulletSpeedMultiplier += upgrade.value; break;
            case UpgradeType.BulletSizeUp: bulletSizeBonus += upgrade.value; break;
            case UpgradeType.FireRateUp: fireRateMultiplier += upgrade.value; break;
            case UpgradeType.ReloadSpeedUp: reloadSpeedMultiplier += upgrade.value; break;
            case UpgradeType.AimUp: critMultiplierBonus += upgrade.value; break;
            case UpgradeType.PiercingBullets: hasPiercingBullets = true; break;
            case UpgradeType.InfiniteAmmo: hasInfiniteAmmo = true; break;

            case UpgradeType.CoinMagnet:
                hasCoinMagnet = true;
                foreach (Coin coin in FindObjectsByType<Coin>(FindObjectsSortMode.None))
                    coin.CheckMagnet();
                break;

            case UpgradeType.ReloadOnKill:
                hasReloadOnKill = true;
                reloadOnKillChance += upgrade.value;
                break;

            case UpgradeType.Execute: hasExecute = true; break;
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
                playerShoot?.ActivateDualWield();
                break;
        }

        UpdateDebugStats();
    }

    // ── Reset al inicio de cada run ───────────────────────────────────────────
    // Limpia los bonuses in-run y aplica los permanentes desde la tienda.
    public void ResetStats()
    {
        // Limpia permanentes anteriores
        _permDamageBonus = 0f;
        _permReloadSpeedBonus = 0f;
        _permMaxHealthBonus = 0f;
        _permArmor = 0f;
        _permLuckBonus = 0f;
        _permXPBonus = 0f;
        _permLootBonus = 0f;

        // Limpia in-run
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        maxHealthBonus = 0f;
        bulletSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
        reloadSpeedMultiplier = 1f;
        luckMultiplier = baseLuck;
        reloadOnKillChance = 0f;
        bulletSizeBonus = 0f;
        critChanceBonus = 0f;
        critMultiplierBonus = 0f;

        hasPiercingBullets = false;
        hasCoinMagnet = false;
        hasReloadOnKill = false;
        hasExecute = false;
        hasDoubleShot = false;
        hasPlagueBullets = false;
        hasPlagueChain = false;
        hasExplosiveBullets = false;
        hasDualWield = false;
        hasTotalPlague = false;
        hasSpecter = false;
        hasLastBullet = false;
        hasGunfightersCurse = false;
        hasInfiniteAmmo = false;

        // Aplica permanentes desde la tienda
        PermanentUpgradeManager.Instance?.ApplyToPlayerStats(this);

        UpdateDebugStats();
    }

    // ── API para PermanentUpgradeManager ─────────────────────────────────────
    public void AddPermanentDamageBonus(float amount)
    {
        _permDamageBonus += amount;
        UpdateDebugStats();
    }

    public void AddPermanentReloadSpeedBonus(float amount)
    {
        _permReloadSpeedBonus += amount;
        UpdateDebugStats();
    }

    public void AddPermanentMaxHealthBonus(float amount)
    {
        _permMaxHealthBonus += amount;
        UpdateDebugStats();
    }

    public void AddPermanentArmor(float amount)
    {
        _permArmor += amount;
        UpdateDebugStats();
    }

    public void AddPermanentLuckBonus(float amount)
    {
        _permLuckBonus += amount;
        UpdateDebugStats();
    }

    public void AddPermanentXPBonus(float amount)
    {
        _permXPBonus += amount;
        UpdateDebugStats();
    }

    public void AddPermanentLootBonus(float amount)
    {
        _permLootBonus += amount;
        UpdateDebugStats();
    }

    // ── Getters in-run ────────────────────────────────────────────────────────
    public void AddCritChance(float amount) { critChanceBonus += amount; UpdateDebugStats(); }
    public void AddCritMultiplier(float amount) { critMultiplierBonus += amount; UpdateDebugStats(); }

    // ── Getters de stats finales ──────────────────────────────────────────────
    // Cada getter combina base + permanente + in-run
    public float GetDamage() => (baseDamage + _permDamageBonus) * damageMultiplier;
    public float GetMoveSpeed() => baseMoveSpeed * moveSpeedMultiplier;
    public float GetMaxHealth() => baseMaxHealth + _permMaxHealthBonus + maxHealthBonus;
    public float GetBulletSpeed() => baseBulletSpeed * bulletSpeedMultiplier;
    public float GetFireRate() => baseFireRate * fireRateMultiplier;
    public float GetReloadSpeed() => (baseReloadSpeed + _permReloadSpeedBonus) * reloadSpeedMultiplier;
    public float GetLuckMultiplier() => luckMultiplier + _permLuckBonus;
    public float GetCritChance() => baseCritChance + critChanceBonus;
    public float GetCritMultiplier() => baseCritMultiplier + critMultiplierBonus;
    public float GetArmor() => _permArmor;
    public float GetXPBonus() => _permXPBonus;
    public float GetLootBonus() => _permLootBonus;
    public float GetReloadOnKillChance() => reloadOnKillChance;
    public float GetExecuteThreshold() => executeThreshold;
    public float GetBulletSizeBonus() => bulletSizeBonus;

    public void ReRegisterUniqueUpgrades()
    {
        if (UpgradePool.Instance == null) return;
        if (hasPiercingBullets) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.PiercingBullets);
        if (hasCoinMagnet) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.CoinMagnet);
        if (hasReloadOnKill) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.ReloadOnKill);
        if (hasExecute) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.Execute);
        if (hasDoubleShot) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.DoubleShot);
        if (hasPlagueBullets) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.PlagueBullets);
        if (hasPlagueChain) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.PlagueChain);
        if (hasExplosiveBullets) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.ExplosiveBullets);
        if (hasDualWield) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.DualWield);
        if (hasTotalPlague) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.TotalPlague);
        if (hasSpecter) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.Specter);
        if (hasLastBullet) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.LastBullet);
        if (hasGunfightersCurse) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.GunfightersCurse);
        if (hasInfiniteAmmo) UpgradePool.Instance.RegisterUniqueUpgrade(UpgradeType.InfiniteAmmo);
    }

    #region Debug
    [Header("Debug — Stats en tiempo real (solo lectura)")]
    [SerializeField] private float _currentDamage;
    [SerializeField] private float _currentMoveSpeed;
    [SerializeField] private float _currentMaxHealth;
    [SerializeField] private float _currentBulletSpeed;
    [SerializeField] private float _currentFireRate;
    [SerializeField] private float _currentReloadSpeed;
    [SerializeField] private float _currentLuck;
    [SerializeField] private float _currentCritChance;
    [SerializeField] private float _currentCritMultiplier;
    [SerializeField] private float _currentReloadOnKillChance;
    [SerializeField] private float _currentArmor;
    [SerializeField] private float _currentXPBonus;
    [SerializeField] private float _currentLootBonus;

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
        _currentReloadOnKillChance = GetReloadOnKillChance();
        _currentArmor = GetArmor();
        _currentXPBonus = GetXPBonus();
        _currentLootBonus = GetLootBonus();
    }
    #endregion
}
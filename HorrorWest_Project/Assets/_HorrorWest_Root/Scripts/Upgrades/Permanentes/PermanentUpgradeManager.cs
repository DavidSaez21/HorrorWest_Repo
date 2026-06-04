using UnityEngine;
using System.Collections.Generic;

public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager Instance { get; private set; }

    [Header("Mejoras permanentes")]
    [SerializeField] private List<PermanentUpgradeData> upgrades = new List<PermanentUpgradeData>();

    [Header("Armas")]
    [SerializeField] private int shotgunCost = 150;
    [SerializeField] private int rifleCost = 300;

    public int SelectedWeaponIndex { get; private set; } = 0;

    public event System.Action OnUpgradesPurchased;
    public event System.Action OnWeaponChanged;

    private const string KEY_SHOTGUN_UNLOCKED = "weapon_shotgun";
    private const string KEY_RIFLE_UNLOCKED = "weapon_rifle";
    private const string KEY_SELECTED_WEAPON = "weapon_selected";
    private const string KEY_UPGRADE_PREFIX = "perm_upgrade_";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SelectedWeaponIndex = PlayerPrefs.GetInt(KEY_SELECTED_WEAPON, 0);
    }

    // ── Niveles ───────────────────────────────────────────────────────────────
    public int GetLevel(PermanentUpgradeData upgrade)
    {
        string key = KEY_UPGRADE_PREFIX + upgrade.upgradeType.ToString();
        return PlayerPrefs.GetInt(key, 0);
    }

    public bool IsPurchased(PermanentUpgradeData upgrade)
        => GetLevel(upgrade) > 0;

    public bool IsMaxLevel(PermanentUpgradeData upgrade)
        => GetLevel(upgrade) >= upgrade.maxLevel;

    public bool CanPurchase(PermanentUpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade);
        if (currentLevel >= upgrade.maxLevel) return false;
        int cost = upgrade.GetCostForLevel(currentLevel + 1);
        return CurrencyManager.Instance != null && CurrencyManager.Instance.GetCoins() >= cost;
    }

    public bool TryPurchaseUpgrade(PermanentUpgradeData upgrade)
    {
        if (!CanPurchase(upgrade)) return false;
        int currentLevel = GetLevel(upgrade);
        int cost = upgrade.GetCostForLevel(currentLevel + 1);
        if (!CurrencyManager.Instance.SpendCoins(cost)) return false;

        string key = KEY_UPGRADE_PREFIX + upgrade.upgradeType.ToString();
        PlayerPrefs.SetInt(key, currentLevel + 1);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        return true;
    }

    // ── Armas ─────────────────────────────────────────────────────────────────
    public bool IsShotgunUnlocked() => PlayerPrefs.GetInt(KEY_SHOTGUN_UNLOCKED, 0) == 1;
    public bool IsRifleUnlocked() => PlayerPrefs.GetInt(KEY_RIFLE_UNLOCKED, 0) == 1;
    public int GetShotgunCost() => shotgunCost;
    public int GetRifleCost() => rifleCost;

    public bool TryPurchaseShotgun()
    {
        if (IsShotgunUnlocked()) return false;
        if (CurrencyManager.Instance == null) return false;
        if (!CurrencyManager.Instance.SpendCoins(shotgunCost)) return false;
        PlayerPrefs.SetInt(KEY_SHOTGUN_UNLOCKED, 1);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        return true;
    }

    public bool TryPurchaseRifle()
    {
        if (IsRifleUnlocked()) return false;
        if (CurrencyManager.Instance == null) return false;
        if (!CurrencyManager.Instance.SpendCoins(rifleCost)) return false;
        PlayerPrefs.SetInt(KEY_RIFLE_UNLOCKED, 1);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        return true;
    }

    public void SelectWeapon(int index)
    {
        if (index == 1 && !IsShotgunUnlocked()) return;
        if (index == 2 && !IsRifleUnlocked()) return;
        SelectedWeaponIndex = index;
        PlayerPrefs.SetInt(KEY_SELECTED_WEAPON, index);
        PlayerPrefs.Save();
        OnWeaponChanged?.Invoke();
    }

    // ── Aplicar a PlayerStats ─────────────────────────────────────────────────
    public void ApplyToPlayerStats(PlayerStats stats)
    {
        if (stats == null) return;
        foreach (PermanentUpgradeData upgrade in upgrades)
        {
            int level = GetLevel(upgrade);
            if (level == 0) continue;
            float value = upgrade.GetTotalValueAtLevel(level);
            ApplySingleUpgrade(stats, upgrade.upgradeType, value);
        }
    }

    private void ApplySingleUpgrade(PlayerStats stats, PermanentUpgradeType type, float value)
    {
        switch (type)
        {
            case PermanentUpgradeType.BaseDamage: stats.AddPermanentDamageBonus(value); break;
            case PermanentUpgradeType.ReloadSpeed: stats.AddPermanentReloadSpeedBonus(value); break;
            case PermanentUpgradeType.CritChance: stats.AddCritChance(value); break;
            case PermanentUpgradeType.CritMultiplier: stats.AddCritMultiplier(value); break;
            case PermanentUpgradeType.MaxHealth: stats.AddPermanentMaxHealthBonus(value); break;
            case PermanentUpgradeType.Armor: stats.AddPermanentArmor(value); break;
            case PermanentUpgradeType.Luck: stats.AddPermanentLuckBonus(value); break;
            case PermanentUpgradeType.ExperienceBonus: stats.AddPermanentXPBonus(value); break;
            case PermanentUpgradeType.LootBonus: stats.AddPermanentLootBonus(value); break;
        }
    }

    public float GetHealOnRunStart()
    {
        PermanentUpgradeData u = GetUpgradeOfType(PermanentUpgradeType.HealOnRunStart);
        return u == null ? 0f : u.GetTotalValueAtLevel(GetLevel(u));
    }

    public float GetHealthRegenPerSecond()
    {
        PermanentUpgradeData u = GetUpgradeOfType(PermanentUpgradeType.HealthRegen);
        return u == null ? 0f : u.GetTotalValueAtLevel(GetLevel(u));
    }

    private PermanentUpgradeData GetUpgradeOfType(PermanentUpgradeType type)
        => upgrades.Find(u => u.upgradeType == type);

    public List<PermanentUpgradeData> GetAllUpgrades() => upgrades;

    // ── Forzar nivel directamente ─────────────────────────────────────────────
    private void ForceLevel(PermanentUpgradeData upgrade, int level)
    {
        if (upgrade == null) return;
        int clamped = Mathf.Clamp(level, 0, upgrade.maxLevel);
        string key = KEY_UPGRADE_PREFIX + upgrade.upgradeType.ToString();
        PlayerPrefs.SetInt(key, clamped);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        Debug.Log($"[PermanentUpgradeManager] {upgrade.upgradeName} forzado a nivel {clamped}");
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private PermanentUpgradeData _debugUpgrade;
    [SerializeField][Range(0, 5)] private int _debugTargetLevel = 0;

    [ContextMenu("Debug — Forzar nivel de mejora")]
    private void DebugForceLevel()
    {
        ForceLevel(_debugUpgrade, _debugTargetLevel);
    }

    [ContextMenu("Debug — Forzar nivel MAX de mejora")]
    private void DebugForceLevelMax()
    {
        if (_debugUpgrade == null) return;
        ForceLevel(_debugUpgrade, _debugUpgrade.maxLevel);
    }

    [ContextMenu("Debug — Forzar nivel 0 de mejora (reset)")]
    private void DebugForceLevelZero()
    {
        ForceLevel(_debugUpgrade, 0);
    }

    [ContextMenu("Debug — Todas las mejoras al MAX")]
    private void DebugAllMax()
    {
        foreach (PermanentUpgradeData upgrade in upgrades)
            ForceLevel(upgrade, upgrade.maxLevel);
    }

    [ContextMenu("Debug — Desbloquear todas las armas")]
    private void DebugUnlockAllWeapons()
    {
        PlayerPrefs.SetInt(KEY_SHOTGUN_UNLOCKED, 1);
        PlayerPrefs.SetInt(KEY_RIFLE_UNLOCKED, 1);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        Debug.Log("[PermanentUpgradeManager] Todas las armas desbloqueadas.");
    }

    [ContextMenu("Debug — Reset todo")]
    private void DebugResetAll()
    {
        foreach (PermanentUpgradeData upgrade in upgrades)
        {
            string key = KEY_UPGRADE_PREFIX + upgrade.upgradeType.ToString();
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.DeleteKey(KEY_SHOTGUN_UNLOCKED);
        PlayerPrefs.DeleteKey(KEY_RIFLE_UNLOCKED);
        PlayerPrefs.DeleteKey(KEY_SELECTED_WEAPON);
        PlayerPrefs.Save();
        OnUpgradesPurchased?.Invoke();
        Debug.Log("[PermanentUpgradeManager] Todo reseteado.");
    }
    #endregion
}
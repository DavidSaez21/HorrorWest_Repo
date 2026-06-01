using UnityEngine;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// PermanentUpgradeManager — gestiona los niveles de todas las mejoras
// permanentes, los guarda en PlayerPrefs y los aplica a PlayerStats
// al inicio de cada run.
//
// Vive en la escena de tienda y persiste entre escenas (DontDestroyOnLoad).
// ─────────────────────────────────────────────────────────────────────────────
public class PermanentUpgradeManager : MonoBehaviour
{
    public static PermanentUpgradeManager Instance { get; private set; }

    [Header("Mejoras permanentes")]
    [Tooltip("Arrastra aquí todos los PermanentUpgradeData en el mismo orden que el enum")]
    [SerializeField] private List<PermanentUpgradeData> upgrades = new List<PermanentUpgradeData>();

    // ── Armas desbloqueadas ───────────────────────────────────────────────────
    [Header("Armas")]
    [SerializeField] private int shotgunCost = 150;
    [SerializeField] private int rifleCost = 300;

    // Arma seleccionada para la run (0=revólver, 1=escopeta, 2=rifle)
    public int SelectedWeaponIndex { get; private set; } = 0;

    // Eventos para que ShopUpgradeCard se actualice
    public event System.Action OnUpgradesPurchased;
    public event System.Action OnWeaponChanged;

    // ── Keys de PlayerPrefs ───────────────────────────────────────────────────
    private const string KEY_SHOTGUN_UNLOCKED = "weapon_shotgun";
    private const string KEY_RIFLE_UNLOCKED = "weapon_rifle";
    private const string KEY_SELECTED_WEAPON = "weapon_selected";
    private const string KEY_UPGRADE_PREFIX = "perm_upgrade_";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SelectedWeaponIndex = PlayerPrefs.GetInt(KEY_SELECTED_WEAPON, 0);
    }

    // ── Niveles de mejoras ────────────────────────────────────────────────────
    public int GetLevel(PermanentUpgradeData upgrade)
    {
        string key = KEY_UPGRADE_PREFIX + upgrade.upgradeType.ToString();
        return PlayerPrefs.GetInt(key, 0);
    }

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
        // Solo puede seleccionar armas desbloqueadas
        if (index == 1 && !IsShotgunUnlocked()) return;
        if (index == 2 && !IsRifleUnlocked()) return;

        SelectedWeaponIndex = index;
        PlayerPrefs.SetInt(KEY_SELECTED_WEAPON, index);
        PlayerPrefs.Save();
        OnWeaponChanged?.Invoke();
    }

    // ── Aplicar stats a PlayerStats al inicio de la run ───────────────────────
    // PlayerStats.ResetStats() llama a esto para aplicar los bonuses permanentes
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
            case PermanentUpgradeType.BaseDamage:
                stats.AddPermanentDamageBonus(value);
                break;
            case PermanentUpgradeType.ReloadSpeed:
                stats.AddPermanentReloadSpeedBonus(value);
                break;
            case PermanentUpgradeType.CritChance:
                stats.AddCritChance(value);
                break;
            case PermanentUpgradeType.CritMultiplier:
                stats.AddCritMultiplier(value);
                break;
            case PermanentUpgradeType.MaxHealth:
                stats.AddPermanentMaxHealthBonus(value);
                break;
            case PermanentUpgradeType.Armor:
                stats.AddPermanentArmor(value);
                break;
            case PermanentUpgradeType.Luck:
                stats.AddPermanentLuckBonus(value);
                break;
            case PermanentUpgradeType.ExperienceBonus:
                stats.AddPermanentXPBonus(value);
                break;
            case PermanentUpgradeType.LootBonus:
                stats.AddPermanentLootBonus(value);
                break;
                // HealOnRunStart y HealthRegen los gestiona directamente
                // PlayerHealth al inicio de la run — se leen desde aquí
        }
    }

    // ── Getters para HealOnRunStart y HealthRegen ─────────────────────────────
    // PlayerHealth los lee directamente al iniciar la run
    public float GetHealOnRunStart()
    {
        PermanentUpgradeData upgrade = GetUpgradeOfType(PermanentUpgradeType.HealOnRunStart);
        if (upgrade == null) return 0f;
        return upgrade.GetTotalValueAtLevel(GetLevel(upgrade));
    }

    public float GetHealthRegenPerSecond()
    {
        PermanentUpgradeData upgrade = GetUpgradeOfType(PermanentUpgradeType.HealthRegen);
        if (upgrade == null) return 0f;
        return upgrade.GetTotalValueAtLevel(GetLevel(upgrade));
    }

    private PermanentUpgradeData GetUpgradeOfType(PermanentUpgradeType type)
    {
        return upgrades.Find(u => u.upgradeType == type);
    }

    public List<PermanentUpgradeData> GetAllUpgrades() => upgrades;

    #region Debug
    [ContextMenu("Debug — Reset todas las mejoras")]
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
        Debug.Log("[PermanentUpgradeManager] Todas las mejoras reseteadas.");
    }
    #endregion
}
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// PermanentUpgradeData — ScriptableObject que define una mejora permanente.
// Crea uno por cada mejora desde el menú Game/PermanentUpgrade.
// ─────────────────────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "PermanentUpgrade", menuName = "Game/PermanentUpgrade")]
public class PermanentUpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName = "Mejora";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Tipo")]
    public PermanentUpgradeType upgradeType;

    [Header("Niveles")]
    [Tooltip("Número máximo de niveles de esta mejora")]
    public int maxLevel = 5;

    [Tooltip("Valor que se añade por cada nivel (ej. 0.1 = +10% por nivel)")]
    public float valuePerLevel = 0.1f;

    [Tooltip("Coste del nivel 1. Los siguientes niveles escalan con costScaleMultiplier")]
    public int baseCost = 10;

    [Tooltip("Multiplicador de coste entre niveles. 2 = doble cada nivel")]
    [Range(1f, 5f)]
    public float costScaleMultiplier = 2f;

    // ── Helpers ───────────────────────────────────────────────────────────────
    /// <summary>Coste de comprar el nivel indicado (1-based).</summary>
    public int GetCostForLevel(int level)
    {
        if (level <= 0 || level > maxLevel) return 0;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costScaleMultiplier, level - 1));
    }

    /// <summary>Valor total acumulado hasta el nivel indicado.</summary>
    public float GetTotalValueAtLevel(int level)
    {
        return valuePerLevel * level;
    }
}

// ── Tipos de mejora permanente ────────────────────────────────────────────────
public enum PermanentUpgradeType
{
    BaseDamage,
    ReloadSpeed,
    CritChance,
    CritMultiplier,
    MaxHealth,
    Armor,
    HealOnRunStart,
    HealthRegen,
    Luck,
    ExperienceBonus,
    LootBonus
}
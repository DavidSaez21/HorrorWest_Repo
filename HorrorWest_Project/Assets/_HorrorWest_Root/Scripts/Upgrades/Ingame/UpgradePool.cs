using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradePool : MonoBehaviour
{
    public static UpgradePool Instance { get; private set; }

    [Header("All Upgrades")]
    [SerializeField] private List<UpgradeData> allUpgrades;

    [Header("Rarity Weights (base)")]
    [SerializeField] private float commonWeight = 60f;
    [SerializeField] private float uncommonWeight = 30f;
    [SerializeField] private float rareWeight = 9f;
    [SerializeField] private float legendaryWeight = 1f;

    // Mejoras únicas ya cogidas esta run
    private HashSet<UpgradeType> pickedUniqueUpgrades = new HashSet<UpgradeType>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Devuelve 3 mejoras aleatorias teniendo en cuenta rareza y suerte
    public List<UpgradeData> GetRandomUpgrades(int count, float luckMultiplier = 1f)
    {
        List<UpgradeData> result = new List<UpgradeData>();
        List<UpgradeData> availablePool = GetAvailableUpgrades();

        if (availablePool.Count == 0) return result;

        // Intentamos count veces sin repetir en las 3 opciones
        List<UpgradeData> chosen = new List<UpgradeData>();
        int maxAttempts = 50;

        while (chosen.Count < count && maxAttempts-- > 0)
        {
            UpgradeRarity rarity = RollRarity(luckMultiplier);
            List<UpgradeData> ofRarity = availablePool
                .Where(u => u.rarity == rarity && !chosen.Contains(u))
                .ToList();

            // Si no hay de esa rareza, coge de cualquiera
            if (ofRarity.Count == 0)
                ofRarity = availablePool.Where(u => !chosen.Contains(u)).ToList();

            if (ofRarity.Count == 0) break;

            chosen.Add(ofRarity[Random.Range(0, ofRarity.Count)]);
        }

        return chosen;
    }

    private List<UpgradeData> GetAvailableUpgrades()
    {
        return allUpgrades
            .Where(u => !(u.isUnique && pickedUniqueUpgrades.Contains(u.upgradeType)))
            .ToList();
    }

    private UpgradeRarity RollRarity(float luckMultiplier)
    {
        float common = commonWeight;
        float uncommon = uncommonWeight;
        float rare = rareWeight * luckMultiplier;
        float legendary = legendaryWeight * luckMultiplier;
        float total = common + uncommon + rare + legendary;

        float roll = Random.Range(0f, total);

        if (roll < common) return UpgradeRarity.Common;
        if (roll < common + uncommon) return UpgradeRarity.Uncommon;
        if (roll < common + uncommon + rare) return UpgradeRarity.Rare;
        return UpgradeRarity.Legendary;
    }

    // Llamado al coger una mejora única
    public void RegisterUniqueUpgrade(UpgradeType type)
    {
        pickedUniqueUpgrades.Add(type);
    }

    // Resetea al morir
    public void ResetPool()
    {
        pickedUniqueUpgrades.Clear();
    }
}
using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public enum UpgradeType
{
    // Repetibles
    DamageUp,
    MoveSpeedUp,
    MaxHealthUp,
    BulletSpeedUp,
    FireRateUp,
    ReloadSpeedUp,

    // Poco comunes
    PiercingBullets,
    CoinMagnet,
    ReloadOnKill,
    DoubleShot,
    HealthRegen,

    // Raras
    PlagueBullets,
    PlagueChain,
    ExplosiveBullets,
    Execute,
    HealOnCrit,

    // Legendarias (únicas)
    DualWield,
    TotalPlague,
    Specter,
    LastBullet,
    GunfightersCurse
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea] public string description;
    [TextArea] public string tooltip;           // El cartelito que aparece al hacer hover
    public Sprite icon;                         // Placeholder de momento

    [Header("Rareza")]
    public UpgradeRarity rarity;

    [Header("Comportamiento")]
    public bool isUnique = false;               // Si es única no puede repetirse en la misma run
    public float value = 0f;                    // El valor numérico de la mejora (ej. 0.15 para +15%)
}
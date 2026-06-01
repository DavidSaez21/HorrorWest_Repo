using UnityEngine;

public enum UpgradeRarity { Common, Uncommon, Rare, Legendary }

public enum UpgradeType
{
    DamageUp, MoveSpeedUp, MaxHealthUp, BulletSpeedUp, FireRateUp,
    ReloadSpeedUp, BulletSizeUp, AimUp,
    PiercingBullets, CoinMagnet, ReloadOnKill, DoubleShot, HealthRegen,
    PlagueBullets, PlagueChain, ExplosiveBullets, Execute, HealOnCrit,
    DualWield, TotalPlague, Specter, LastBullet, GunfightersCurse, InfiniteAmmo
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea] public string description;
    [TextArea] public string tooltip;

    [Header("Icons")]
    public Sprite icon;             // Icono que aparece en la tarjeta de selección
    public Sprite activeIcon;       // Icono que aparece en el panel de mejoras activas (esquina superior derecha)

    [Header("Rareza")]
    public UpgradeRarity rarity;

    [Header("Comportamiento")]
    public bool isUnique = false;
    public float value = 0f;
}
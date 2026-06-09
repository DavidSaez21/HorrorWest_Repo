using UnityEngine;

/// <summary>
/// EnemyData extension for the Church Boss.
/// Solo daños — las duraciones se configuran en cada componente de ataque.
/// Create via: Assets > Create > Game > Church Boss Data
/// </summary>
[CreateAssetMenu(fileName = "ChurchBossData", menuName = "Game/Church Boss Data")]
public class ChurchBossData : EnemyData
{
    [Header("── Daño de ataques ──────────────────────────")]
    public float tongueBiteDamage = 25f;
    public float tongueSweepDamage = 20f;
    public float tentacleDamage = 18f;
    public float mouthBiteDamage = 30f;
    public float mouthOpenDPS = 10f;

    [Header("── Minion Spawner ──────────────────────────────")]
    public float minionSpawnStagger = 0.3f;
}
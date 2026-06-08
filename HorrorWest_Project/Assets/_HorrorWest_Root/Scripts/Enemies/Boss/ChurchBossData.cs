using UnityEngine;

/// <summary>
/// EnemyData extension for the Church Boss.
/// Todos los daños, pesos y cooldowns configurables desde el Inspector.
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

    [Header("── Tongue Bite ────────────────────────────────")]
    public float tongueBiteDuration = 0.8f;

    [Header("── Tongue Sweep ───────────────────────────────")]
    public float tongueSweepDuration = 1.0f;

    [Header("── Tentacle Attack ──────────────────────────────")]
    public float tentacleEmergeDuration = 0.8f;
    public float tentacleWaitAtTargetDuration = 0.5f;
    public float tentacleAttackAnimDuration = 0.6f;
    public float tentacleRetractDuration = 0.5f;

    [Header("── Ground Mouth Attack ────────────────────────")]
    public float mouthCrackDuration = 0.6f;
    public float mouthOpenDuration = 0.8f;
    public float mouthBiteDuration = 0.3f;

    [Header("── Minion Spawner ──────────────────────────────")]
    public float minionSpawnStagger = 0.3f;
}
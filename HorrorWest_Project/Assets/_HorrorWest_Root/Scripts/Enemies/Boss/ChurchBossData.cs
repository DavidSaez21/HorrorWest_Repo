using UnityEngine;

/// <summary>
/// EnemyData extension for the Church Boss.
/// Inherits all base stats (maxHealth, damage, defense, xpReward, dropChance…)
/// and adds per-attack damage overrides + tuning knobs exposed in the Inspector.
/// Create via: Assets > Create > Game > Church Boss Data
/// </summary>
[CreateAssetMenu(fileName = "ChurchBossData", menuName = "Game/Church Boss Data")]
public class ChurchBossData : EnemyData
{
    [Header("── Attack Damage ──────────────────────────")]
    [Tooltip("Damage dealt by the Tongue lunge on contact.")]
    public float tongueDamage = 25f;

    [Tooltip("Damage dealt per tentacle sweep hit.")]
    public float tentacleDamage = 18f;

    [Tooltip("Damage dealt when a ground mouth closes on the player (bite).")]
    public float mouthBiteDamage = 30f;

    [Tooltip("Damage dealt while standing inside an open ground mouth (per second).")]
    public float mouthOpenDPS = 10f;

    [Header("── Tongue Attack ────────────────────────────")]
    [Tooltip("How long the tongue retracts (windup) before lunging.")]
    public float tongueWindupDuration = 0.5f;

    [Tooltip("How long the tongue takes to travel the full lunge distance.")]
    public float tongueLungeDuration = 0.25f;

    [Tooltip("How long the tongue lingers at full extension before retracting.")]
    public float tongueLingerDuration = 0.15f;

    [Tooltip("How long the tongue takes to retract back to the boss.")]
    public float tongueRetractDuration = 0.4f;

    [Tooltip("Max distance the tongue travels south from its origin.")]
    public float tongueLungeDistance = 8f;

    [Header("── Tentacle Attack ──────────────────────────")]
    [Tooltip("How long the tentacle tip is visible before sweeping (player telegraph window).")]
    public float tentacleTelegraphDuration = 0.4f;

    [Tooltip("How long the sweep across the aisle takes.")]
    public float tentacleSweepDuration = 0.35f;

    [Tooltip("How long before the tentacle fully retracts after sweeping.")]
    public float tentacleRetractDuration = 0.5f;

    [Header("── Ground Mouth Attack ────────────────────────")]
    [Tooltip("Duration of the closed crack appearing in the ground (phase 1 telegraph).")]
    public float mouthCrackDuration = 0.6f;

    [Tooltip("Duration the mouth stays fully open — hitbox active.")]
    public float mouthOpenDuration = 0.8f;

    [Tooltip("Duration of the closing bite animation.")]
    public float mouthBiteDuration = 0.3f;

    [Header("── Minion Spawner ──────────────────────────────")]
    [Tooltip("Delay between each minion appearing at the door (staggered entry).")]
    public float minionSpawnStagger = 0.3f;
}

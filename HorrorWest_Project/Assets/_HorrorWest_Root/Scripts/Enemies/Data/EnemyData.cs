using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public float defense = 0f;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("Experience & Drops")]
    public float xpReward = 20f;
    public float dropChance = 0.8f;
    public float dropChanceCoinOne = 70f;
    public float dropChanceCoinFive = 25f;
    public float dropChanceCoinTen = 5f;
}    
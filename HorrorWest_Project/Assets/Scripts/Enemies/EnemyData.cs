using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName = "Enemy";
    public Sprite icon;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public float defense = 0f;              // Reducción de daño recibido (0 = sin armadura)

    [Header("Detection")]
    public float detectionRange = 8f;       // Distancia a la que detecta al player
    public float attackRange = 1.5f;        // Distancia a la que puede atacar
    public float attackCooldown = 1f;       // Segundos entre ataques

    [Header("Experience & Drops")]
    public float xpReward = 20f;
    public float dropChance = 0.8f;
    public float dropChanceCoinOne = 70f;
    public float dropChanceCoinFive = 25f;
    public float dropChanceCoinTen = 5f;
}
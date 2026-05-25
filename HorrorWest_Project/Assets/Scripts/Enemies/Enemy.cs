using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Experience")]
    [SerializeField] private float xpReward = 20f;      // XP que da al morir, ajustar por tipo de enemigo

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] coinPrefabs;
    [SerializeField] private float dropChance = 0.8f;
    [SerializeField] private float dropChanceCoinOne = 70f;
    [SerializeField] private float dropChanceCoinFive = 25f;
    [SerializeField] private float dropChanceCoinTen = 5f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        ExperienceManager.Instance.AddXP(xpReward);
        TryDropCoin();
        Destroy(gameObject);
    }

    private void TryDropCoin()
    {
        if (coinPrefabs == null || coinPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        float total = dropChanceCoinOne + dropChanceCoinFive + dropChanceCoinTen;
        float roll = Random.Range(0f, total);

        int index;
        if (roll < dropChanceCoinOne) index = 0;
        else if (roll < dropChanceCoinOne + dropChanceCoinFive) index = 1;
        else index = 2;

        index = Mathf.Clamp(index, 0, coinPrefabs.Length - 1);

        if (coinPrefabs[index] != null)
            Instantiate(coinPrefabs[index], transform.position, Quaternion.identity);
    }
}
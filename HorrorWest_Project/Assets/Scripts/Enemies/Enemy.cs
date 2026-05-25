using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] coinPrefabs;  // 0 = moneda x1, 1 = moneda x5, 2 = moneda x10
    [SerializeField] private float dropChance = 0.8f;   // Probabilidad de dropear algo
    [SerializeField] private float dropChanceCoinOne = 70f;     // % moneda x1
    [SerializeField] private float dropChanceCoinFive = 25f;    // % moneda x5
    [SerializeField] private float dropChanceCoinTen = 5f;      // % moneda x10

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
        if (roll < dropChanceCoinOne) index = 0;  // x1
        else if (roll < dropChanceCoinOne + dropChanceCoinFive) index = 1;  // x5
        else index = 2;  // x10

        index = Mathf.Clamp(index, 0, coinPrefabs.Length - 1);

        if (coinPrefabs[index] != null)
            Instantiate(coinPrefabs[index], transform.position, Quaternion.identity);
    }
}
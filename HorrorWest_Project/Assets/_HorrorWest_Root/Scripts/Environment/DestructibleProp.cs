using UnityEngine;

public class DestructibleProp : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 30f;
    private float currentHealth;

    [Header("Drop Settings")]
    [SerializeField] private GameObject[] coinPrefabs;  // 0 = x1, 1 = x5, 2 = x10
    [SerializeField] private float dropChance = 0.5f;
    [SerializeField] private float dropChanceCoinOne = 80f;
    [SerializeField] private float dropChanceCoinFive = 18f;
    [SerializeField] private float dropChanceCoinTen = 2f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
            Break();
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        TakeDamage(amount);
    }

    private void Break()
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
        if (roll < dropChanceCoinOne) index = 0;
        else if (roll < dropChanceCoinOne + dropChanceCoinFive) index = 1;
        else index = 2;

        index = Mathf.Clamp(index, 0, coinPrefabs.Length - 1);

        if (coinPrefabs[index] != null)
            Instantiate(coinPrefabs[index], transform.position, Quaternion.identity);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;           // Igual que la barra de XP, Image con Fill Method

    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime = 0.5f;  // Segundos de invencibilidad tras recibir daño
    private float lastDamageTime = -999f;

    // Eventos para conectar animaciones y otros sistemas
    public event System.Action<float, float> OnHealthChanged;   // vida actual, vida máxima
    public event System.Action OnDeath;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        // Invencibilidad temporal tras recibir daño
        if (Time.time < lastDamageTime + invincibilityTime) return;

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - amount);

        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Player ha muerto");
        // Aquí conectaremos el menú de tienda cuando lo hagamos
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;
    }

    // Llamado desde PlayerStats al aplicar mejoras de vida máxima
    public void SetMaxHealth(float newMax)
    {
        float ratio = currentHealth / maxHealth;
        maxHealth = newMax;
        currentHealth = maxHealth * ratio;
        UpdateUI();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
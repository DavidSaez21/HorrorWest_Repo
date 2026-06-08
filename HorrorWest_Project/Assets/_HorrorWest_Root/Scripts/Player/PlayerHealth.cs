using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityTime = 0.5f;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action OnDeath;

    private float currentHealth;
    private float lastDamageTime = -999f;
    private PlayerHitFlash _hitFlash;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
        _hitFlash = GetComponentInChildren<PlayerHitFlash>(true);
    }

    public void TakeDamage(float amount)
    {
        if (Time.time < lastDamageTime + invincibilityTime) return;
        lastDamageTime = Time.time;

        float armor = PlayerManager.Instance?.Stats?.GetArmor() ?? 0f;
        float reduced = Mathf.Max(0f, amount - armor);
        currentHealth = Mathf.Max(0f, currentHealth - reduced);

        CameraShaker.Instance?.Shake(0.08f, 0.15f);
        _hitFlash?.StartInvincibilityFlash(invincibilityTime);

        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax)
    {
        float ratio = currentHealth / maxHealth;
        maxHealth = newMax;
        currentHealth = maxHealth * ratio;
        UpdateUI();
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("[PlayerHealth] Player ha muerto.");
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugLogDamage = false;

    public void DebugTakeDamage(float amount)
    {
        if (!debugLogDamage) return;
        Debug.Log($"[PlayerHealth] Daño recibido: {amount} | Vida: {currentHealth} / {maxHealth}");
    }
    #endregion
}
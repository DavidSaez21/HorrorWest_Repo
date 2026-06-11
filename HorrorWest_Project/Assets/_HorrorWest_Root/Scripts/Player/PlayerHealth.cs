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
    [SerializeField] private Image ghostBarFill;
    [SerializeField] private float ghostDelay = 0.5f;
    [SerializeField] private float ghostSpeed = 0.8f;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action OnDeath;

    private float currentHealth;
    private float lastDamageTime = -999f;
    private float _ghostAmount = 1f;
    private float _delayTimer = 0f;
    private bool _draining = false;
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

        // Busca las barras de vida en el HUD siempre (el Player puede ser nuevo)
        StartCoroutine(FindHealthBars());
    }

    private System.Collections.IEnumerator FindHealthBars()
    {
        yield return new WaitUntil(() => PersistentCanvas.HUDInstance != null);

        // Busca por nombre dentro del HUDCanva
        Transform hud = PersistentCanvas.HUDInstance.transform;

        if (healthBarFill == null)
        {
            Transform fill = FindDeepChild(hud, "Fill");
            if (fill != null) healthBarFill = fill.GetComponent<Image>();
        }

        if (ghostBarFill == null)
        {
            Transform gris = FindDeepChild(hud, "Gris");
            if (gris != null) ghostBarFill = gris.GetComponent<Image>();
        }

        UpdateUI();
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindDeepChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    private void Update()
    {
        if (ghostBarFill == null) return;
        float targetFill = healthBarFill != null ? healthBarFill.fillAmount : 0f;
        if (_ghostAmount <= targetFill) return;
        if (!_draining)
        {
            _delayTimer -= Time.deltaTime;
            if (_delayTimer <= 0f) _draining = true;
        }
        else
        {
            _ghostAmount = Mathf.MoveTowards(_ghostAmount, targetFill, ghostSpeed * Time.deltaTime);
            ghostBarFill.fillAmount = _ghostAmount;
        }
    }

    public void TakeDamage(float amount)
    {
        if (Time.time < lastDamageTime + invincibilityTime) return;
        lastDamageTime = Time.time;
        float armor = PlayerManager.Instance?.Stats?.GetArmor() ?? 0f;
        float reduced = amount * (1f - Mathf.Clamp01(armor));
        currentHealth = Mathf.Max(0f, currentHealth - reduced);
        CameraShaker.Instance?.Shake(0.08f, 0.15f);
        _hitFlash?.StartInvincibilityFlash(invincibilityTime);
        _delayTimer = ghostDelay;
        _draining = false;
        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0f) Die();
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
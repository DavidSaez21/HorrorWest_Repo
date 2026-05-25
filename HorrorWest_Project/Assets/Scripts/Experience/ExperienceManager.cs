using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private float baseXPRequired = 100f;
    [SerializeField] private float exponentialFactor = 1.4f;

    private int currentLevel = 1;
    private float currentXP = 0f;
    private float xpToNextLevel;

    public event System.Action<float, float> OnXPChanged;
    public event System.Action<int> OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        xpToNextLevel = CalculateXPForLevel(currentLevel);
        Debug.Log($"ExperienceManager iniciado. XP para nivel 1: {xpToNextLevel}");
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        Debug.Log($"XP añadida: {amount} | XP actual: {currentXP} / {xpToNextLevel}");
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = CalculateXPForLevel(currentLevel);
            Debug.Log($"¡Nivel {currentLevel} alcanzado!");
            OnLevelUp?.Invoke(currentLevel);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        }
    }

    private float CalculateXPForLevel(int level)
    {
        return baseXPRequired * Mathf.Pow(exponentialFactor, level - 1);
    }

    public void ResetXP()
    {
        currentLevel = 1;
        currentXP = 0f;
        xpToNextLevel = CalculateXPForLevel(currentLevel);
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentXP() => currentXP;
    public float GetXPToNextLevel() => xpToNextLevel;
}
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private int totalCoins = 0;

    // Evento para que la UI se actualice automáticamente al cambiar las monedas
    public event System.Action<int> OnCoinsChanged;

    private void Awake()
    {
        // Singleton — persiste entre escenas (lobby y niveles)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Carga las monedas guardadas si las hay
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        OnCoinsChanged?.Invoke(totalCoins);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins < amount) return false;

        totalCoins -= amount;
        OnCoinsChanged?.Invoke(totalCoins);
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        return true;
    }

    public int GetCoins() => totalCoins;
}
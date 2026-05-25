using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        // Muestra las monedas actuales al arrancar
        UpdateUI(CurrencyManager.Instance.GetCoins());

        // Se suscribe al evento para actualizarse automáticamente
        CurrencyManager.Instance.OnCoinsChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        // Limpia la suscripción al destruirse
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= UpdateUI;
    }

    private void UpdateUI(int amount)
    {
        coinText.text = $"{amount}";
    }
}
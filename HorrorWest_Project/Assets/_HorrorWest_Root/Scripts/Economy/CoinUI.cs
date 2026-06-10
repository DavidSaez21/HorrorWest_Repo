using System.Collections;
using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private IEnumerator Start()
    {
        yield return null; // espera un frame a que el CurrencyManager esté listo
        if (CurrencyManager.Instance != null)
        {
            UpdateUI(CurrencyManager.Instance.GetCoins());
            CurrencyManager.Instance.OnCoinsChanged += UpdateUI;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= UpdateUI;
    }

    private void UpdateUI(int amount)
    {
        if (coinText != null)
            coinText.text = $"{amount}";
    }
}
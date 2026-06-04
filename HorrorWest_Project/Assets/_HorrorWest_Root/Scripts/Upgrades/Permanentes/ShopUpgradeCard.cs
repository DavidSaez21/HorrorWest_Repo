using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUpgradeCard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PermanentUpgradeData upgradeData;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;       // Nivel actual
    [SerializeField] private TextMeshProUGUI romanLevelText;  // I II III IV V
    [SerializeField] private TextMeshProUGUI priceText;       // Precio o ✗
    [SerializeField] private Button buyButton;
    [SerializeField] private Image cardBackground;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color maxLevelColor = new Color(0.05f, 0.3f, 0.05f);
    [SerializeField] private Color cantAffordColor = new Color(0.3f, 0.1f, 0.1f);

    // Numeración romana hasta 5 niveles
    private static readonly string[] RomanNumerals = { "", "I", "II", "III", "IV", "V" };

    private void Start()
    {
        if (upgradeData == null)
        {
            Debug.LogError($"[ShopUpgradeCard] {gameObject.name} no tiene UpgradeData asignado.");
            return;
        }

        if (iconImage != null && upgradeData.icon != null)
            iconImage.sprite = upgradeData.icon;
        if (nameText != null)
            nameText.text = upgradeData.upgradeName;
        if (descriptionText != null)
            descriptionText.text = upgradeData.description;

        buyButton?.onClick.AddListener(OnBuyClicked);

        if (PermanentUpgradeManager.Instance != null)
            PermanentUpgradeManager.Instance.OnUpgradesPurchased += Refresh;

        Refresh();
    }

    private void OnDestroy()
    {
        if (PermanentUpgradeManager.Instance != null)
            PermanentUpgradeManager.Instance.OnUpgradesPurchased -= Refresh;
    }

    public void Refresh()
    {
        if (upgradeData == null || PermanentUpgradeManager.Instance == null) return;

        int currentLevel = PermanentUpgradeManager.Instance.GetLevel(upgradeData);
        bool isMaxLevel = currentLevel >= upgradeData.maxLevel;
        bool canAfford = PermanentUpgradeManager.Instance.CanPurchase(upgradeData);

        // Nivel numérico
        if (levelText != null)
            levelText.text = isMaxLevel
                ? $"{upgradeData.maxLevel} / {upgradeData.maxLevel}"
                : $"{currentLevel} / {upgradeData.maxLevel}";

        // Nivel en romano — muestra el nivel actual (o MAX si está al máximo)
        if (romanLevelText != null)
        {
            int clampedLevel = Mathf.Clamp(currentLevel, 0, RomanNumerals.Length - 1);
            romanLevelText.text = isMaxLevel
                ? RomanNumerals[upgradeData.maxLevel <= RomanNumerals.Length - 1 ? upgradeData.maxLevel : RomanNumerals.Length - 1]
                : (currentLevel == 0 ? "—" : RomanNumerals[clampedLevel]);
        }

        // Precio o ✗ si está al máximo
        if (priceText != null)
            priceText.text = isMaxLevel
                ? " ✗"
                : $"{upgradeData.GetCostForLevel(currentLevel + 1)} $";

        // Botón
        if (buyButton != null)
            buyButton.interactable = !isMaxLevel && canAfford;

        // Color del fondo
        if (cardBackground != null)
        {
            if (isMaxLevel) cardBackground.color = maxLevelColor;
            else if (!canAfford) cardBackground.color = cantAffordColor;
            else cardBackground.color = normalColor;
        }
    }

    private void OnBuyClicked()
    {
        if (upgradeData == null || PermanentUpgradeManager.Instance == null) return;
        PermanentUpgradeManager.Instance.TryPurchaseUpgrade(upgradeData);
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugLogPurchase = false;
    #endregion
}
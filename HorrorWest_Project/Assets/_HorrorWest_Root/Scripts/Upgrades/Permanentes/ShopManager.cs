using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Navegación")]
    [SerializeField] private string gameSceneName = "Level_1";
    [SerializeField] private string menuSceneName = "SCN_Menu";

    [Header("Monedas")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Armas")]
    [SerializeField] private WeaponShopCard revolverCard;
    [SerializeField] private WeaponShopCard shotgunCard;
    [SerializeField] private WeaponShopCard rifleCard;

    [Header("Mejoras")]
    [SerializeField] private ShopUpgradeCard[] upgradeCards;

    [Header("Botones")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;
    [SerializeField] private ButtonSound buySound;

    private void Start()
    {
        playButton?.onClick.AddListener(OnPlayClicked);
        backButton?.onClick.AddListener(OnBackClicked);

        UpdateCoinsText();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += _ => UpdateCoinsText();

        SetupWeaponCards();

        revolverCard?.SetBuySound(buySound);
        shotgunCard?.SetBuySound(buySound);
        rifleCard?.SetBuySound(buySound);
        foreach (var card in upgradeCards)
            card?.SetBuySound(buySound);

        if (PermanentUpgradeManager.Instance != null)
            PermanentUpgradeManager.Instance.OnWeaponChanged += RefreshWeaponCards;
    }

    private void OnDestroy()
    {
        if (PermanentUpgradeManager.Instance != null)
            PermanentUpgradeManager.Instance.OnWeaponChanged -= RefreshWeaponCards;
    }

    private void UpdateCoinsText()
    {
        if (coinsText == null || CurrencyManager.Instance == null) return;
        coinsText.text = $"{CurrencyManager.Instance.GetCoins()} $";
    }

    private void SetupWeaponCards()
    {
        if (PermanentUpgradeManager.Instance == null) return;

        revolverCard?.Setup(
            weaponIndex: 0,
            isUnlocked: true,
            price: 0,
            onBuy: null,
            onSelect: () => PermanentUpgradeManager.Instance.SelectWeapon(0)
        );

        shotgunCard?.Setup(
            weaponIndex: 1,
            isUnlocked: PermanentUpgradeManager.Instance.IsShotgunUnlocked(),
            price: PermanentUpgradeManager.Instance.GetShotgunCost(),
            onBuy: TryBuyShotgun,
            onSelect: () => PermanentUpgradeManager.Instance.SelectWeapon(1)
        );

        rifleCard?.Setup(
            weaponIndex: 2,
            isUnlocked: PermanentUpgradeManager.Instance.IsRifleUnlocked(),
            price: PermanentUpgradeManager.Instance.GetRifleCost(),
            onBuy: TryBuyRifle,
            onSelect: () => PermanentUpgradeManager.Instance.SelectWeapon(2)
        );

        RefreshWeaponCards();
    }

    private void RefreshWeaponCards()
    {
        if (PermanentUpgradeManager.Instance == null) return;
        int selected = PermanentUpgradeManager.Instance.SelectedWeaponIndex;
        revolverCard?.SetSelected(selected == 0);
        shotgunCard?.SetSelected(selected == 1);
        rifleCard?.SetSelected(selected == 2);
    }

    private void TryBuyShotgun()
    {
        if (PermanentUpgradeManager.Instance == null) return;
        if (PermanentUpgradeManager.Instance.TryPurchaseShotgun())
        {
            shotgunCard?.SetUnlocked();
            RefreshWeaponCards();
            UpdateCoinsText();
        }
    }

    private void TryBuyRifle()
    {
        if (PermanentUpgradeManager.Instance == null) return;
        if (PermanentUpgradeManager.Instance.TryPurchaseRifle())
        {
            rifleCard?.SetUnlocked();
            RefreshWeaponCards();
            UpdateCoinsText();
        }
    }

    private void OnPlayClicked() => SceneManager.LoadScene(gameSceneName);
    private void OnBackClicked() => SceneManager.LoadScene(menuSceneName);

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    [ContextMenu("Debug — Añadir 100 monedas")]
    private void DebugAdd100() => CurrencyManager.Instance?.AddCoins(100);

    [ContextMenu("Debug — Añadir 500 monedas")]
    private void DebugAdd500() => CurrencyManager.Instance?.AddCoins(500);

    [ContextMenu("Debug — Añadir 1000 monedas")]
    private void DebugAdd1000() => CurrencyManager.Instance?.AddCoins(1000);

    [ContextMenu("Debug — Reset monedas")]
    private void DebugResetCoins()
    {
        PlayerPrefs.SetInt("TotalCoins", 0);
        PlayerPrefs.Save();
        UpdateCoinsText();
    }
    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponShopCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;      // Precio si bloqueada, X si seleccionada, nada si desbloqueada no seleccionada
    [SerializeField] private Button actionButton;
    [SerializeField] private Image selectedBorder;

    [Header("Colors")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;

    private int _weaponIndex;
    private bool _isUnlocked;
    private int _price;
    private System.Action _onBuy;
    private System.Action _onSelect;

    public void Setup(int weaponIndex, bool isUnlocked, int price, System.Action onBuy, System.Action onSelect)
    {
        _weaponIndex = weaponIndex;
        _isUnlocked = isUnlocked;
        _price = price;
        _onBuy = onBuy;
        _onSelect = onSelect;

        actionButton?.onClick.RemoveAllListeners();

        if (priceText != null) priceText.text = "";

        if (_isUnlocked)
            SetupUnlocked();
        else
            SetupLocked();
    }

    private void SetupUnlocked()
    {
        _isUnlocked = true;

        // Sin precio — el texto se limpia, solo aparece X al seleccionar
        if (priceText != null) priceText.text = "";
        if (iconImage != null) iconImage.color = unlockedColor;

        actionButton?.onClick.AddListener(() => _onSelect?.Invoke());
        if (actionButton != null) actionButton.interactable = true;
    }

    private void SetupLocked()
    {
        // Muestra el precio directamente
        if (priceText != null) priceText.text = $"{_price} $";
        if (iconImage != null) iconImage.color = lockedColor;

        actionButton?.onClick.AddListener(OnActionClicked);
        RefreshAffordability();
    }

    private void RefreshAffordability()
    {
        if (actionButton == null) return;
        bool canAfford = CurrencyManager.Instance != null
            && CurrencyManager.Instance.GetCoins() >= _price;
        actionButton.interactable = canAfford;
    }

    // ── Selección ─────────────────────────────────────────────────────────────
    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.gameObject.SetActive(isSelected);

        // X si seleccionada, nada si desbloqueada pero no seleccionada
        // Si está bloqueada mantiene el precio
        if (priceText != null && _isUnlocked)
            priceText.text = isSelected ? " X" : "";
    }

    public void SetUnlocked()
    {
        _isUnlocked = true;
        actionButton?.onClick.RemoveAllListeners();
        SetupUnlocked();
    }

    private void OnActionClicked()
    {
        if (_isUnlocked) _onSelect?.Invoke();
        else _onBuy?.Invoke();
    }

    #region Debug
    protected void OnDrawGizmosSelected() { }
    #endregion
}
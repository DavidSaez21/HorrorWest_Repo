using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
// WeaponShopCard — card de arma en la tienda.
// Muestra el estado del arma: seleccionada, desbloqueada o con precio.
// ─────────────────────────────────────────────────────────────────────────────
public class WeaponShopCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;     // "SELECCIONADA", "COMPRAR 150$", etc.
    [SerializeField] private Button actionButton;   // Comprar o Seleccionar
    [SerializeField] private Image selectedBorder; // Borde que se activa al seleccionar

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;

    private int _weaponIndex;
    private bool _isUnlocked;
    private System.Action _onBuy;
    private System.Action _onSelect;

    // ── Setup ─────────────────────────────────────────────────────────────────
    public void Setup(int weaponIndex, bool isUnlocked, int price, System.Action onBuy, System.Action onSelect)
    {
        _weaponIndex = weaponIndex;
        _isUnlocked = isUnlocked;
        _onBuy = onBuy;
        _onSelect = onSelect;

        actionButton?.onClick.RemoveAllListeners();

        if (_isUnlocked)
            SetupUnlocked();
        else
            SetupLocked(price);
    }

    private void SetupUnlocked()
    {
        _isUnlocked = true;

        if (statusText != null) statusText.text = "SELECCIONAR";
        if (iconImage != null) iconImage.color = unlockedColor;

        actionButton?.onClick.AddListener(() => _onSelect?.Invoke());
        if (actionButton != null) actionButton.interactable = true;
    }

    private void SetupLocked(int price)
    {
        if (statusText != null) statusText.text = $"COMPRAR\n{price} $";
        if (iconImage != null) iconImage.color = lockedColor;

        // Botón de compra — solo si hay suficientes monedas
        actionButton?.onClick.AddListener(OnActionClicked);
        RefreshAffordability(price);
    }

    private void RefreshAffordability(int price)
    {
        if (actionButton == null) return;
        bool canAfford = CurrencyManager.Instance != null
            && CurrencyManager.Instance.GetCoins() >= price;
        actionButton.interactable = canAfford;
    }

    // ── Estado ────────────────────────────────────────────────────────────────
    public void SetSelected(bool isSelected)
    {
        if (selectedBorder != null)
            selectedBorder.gameObject.SetActive(isSelected);

        if (statusText != null && _isUnlocked)
            statusText.text = isSelected ? "SELECCIONADA" : "SELECCIONAR";
    }

    public void SetUnlocked()
    {
        _isUnlocked = true;
        actionButton?.onClick.RemoveAllListeners();
        SetupUnlocked();
    }

    // ── Acción ────────────────────────────────────────────────────────────────
    private void OnActionClicked()
    {
        if (_isUnlocked)
            _onSelect?.Invoke();
        else
            _onBuy?.Invoke();
    }

    #region Debug
    protected void OnDrawGizmosSelected() { }
    #endregion
}
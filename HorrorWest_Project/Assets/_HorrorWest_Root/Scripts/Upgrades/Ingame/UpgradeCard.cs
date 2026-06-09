using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RarityBorderAnimator rarityAnimator;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverSpeed = 8f;

    private UpgradeData _data;
    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private System.Action<UpgradeData> _onSelected;
    private bool _interactable = false;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, hoverSpeed * Time.unscaledDeltaTime);
    }

    public void Setup(UpgradeData data, System.Action<UpgradeData> onSelected)
    {
        _data = data;
        _onSelected = onSelected;

        if (nameText != null) nameText.text = data.upgradeName;
        if (descriptionText != null) descriptionText.text = data.description;
        if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
        if (tooltipText != null) tooltipText.text = data.tooltip;
        if (rarityAnimator != null) rarityAnimator.SetRarity(data.rarity);
    }

    public void SetInteractable(bool value) => _interactable = value;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[UpgradeCard] OnPointerEnter — {gameObject.name} — interactable: {_interactable}");
        if (!_interactable) return;
        _targetScale = _originalScale * hoverScale;
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _originalScale;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable || _data == null) return;
        _onSelected?.Invoke(_data);
    }
}
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
    [SerializeField] private Image rarityBorder;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverSpeed = 8f;

    private UpgradeData data;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private System.Action<UpgradeData> onSelected;

    private static readonly Color ColorCommon = new Color(0.75f, 0.75f, 0.75f);
    private static readonly Color ColorUncommon = new Color(0.12f, 0.56f, 1f);
    private static readonly Color ColorRare = new Color(0.63f, 0.13f, 0.94f);
    private static readonly Color ColorLegendary = new Color(1f, 0.75f, 0f);

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        // Asegura que la tarjeta tenga un Image con Raycast Target para detectar clicks
        if (GetComponent<Image>() == null)
        {
            Image img = gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Transparente
            img.raycastTarget = true;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, hoverSpeed * Time.unscaledDeltaTime);
    }

    public void Setup(UpgradeData upgradeData, System.Action<UpgradeData> onSelectedCallback)
    {
        data = upgradeData;
        onSelected = onSelectedCallback;

        if (nameText != null) nameText.text = data.upgradeName;
        if (descriptionText != null) descriptionText.text = data.description;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (rarityBorder != null)
            rarityBorder.color = GetRarityColor(data.rarity);

        if (tooltipText != null)
            tooltipText.text = data.tooltip;
    }

    // IPointerClickHandler funciona aunque timeScale sea 0
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClick - data: {(data == null ? "NULL" : data.upgradeName)}");
        if (data == null) return;
        onSelected?.Invoke(data);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        if (tooltipPanel != null)
            tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    private Color GetRarityColor(UpgradeRarity rarity)
    {
        return rarity switch
        {
            UpgradeRarity.Common => ColorCommon,
            UpgradeRarity.Uncommon => ColorUncommon,
            UpgradeRarity.Rare => ColorRare,
            UpgradeRarity.Legendary => ColorLegendary,
            _ => Color.white
        };
    }
}
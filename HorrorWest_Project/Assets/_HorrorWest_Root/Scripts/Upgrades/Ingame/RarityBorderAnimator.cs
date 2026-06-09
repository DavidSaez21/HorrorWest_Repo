using UnityEngine;
using UnityEngine.UI;

public class RarityBorderAnimator : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Image borderImage;
    [SerializeField] private UpgradeRarity rarity;

    [Header("Animación")]
    [SerializeField] private float shimmerSpeed = 1.5f;
    [SerializeField] private float shimmerIntensity = 0.15f; // cuanto varía el brillo

    // Colores base por rareza
    private static readonly Color ColorCommon = new Color(0.6f, 0.6f, 0.6f);
    private static readonly Color ColorUncommon = new Color(0.12f, 0.56f, 1f);
    private static readonly Color ColorRare = new Color(0.63f, 0.13f, 0.94f);
    private static readonly Color ColorLegendary = new Color(1f, 0.75f, 0f);

    private Color _baseColor;

    private void Awake()
    {
        if (borderImage == null)
            borderImage = GetComponent<Image>();

        _baseColor = GetBaseColor(rarity);
    }

    private void Update()
    {
        float shimmer = Mathf.Sin(Time.unscaledTime * shimmerSpeed * Mathf.PI) * shimmerIntensity;
        Color animated = new Color(
            Mathf.Clamp01(_baseColor.r + shimmer),
            Mathf.Clamp01(_baseColor.g + shimmer),
            Mathf.Clamp01(_baseColor.b + shimmer),
            1f
        );
        borderImage.color = animated;
    }

    public void SetRarity(UpgradeRarity newRarity)
    {
        rarity = newRarity;
        _baseColor = GetBaseColor(rarity);
    }

    private Color GetBaseColor(UpgradeRarity r)
    {
        return r switch
        {
            UpgradeRarity.Common => ColorCommon,
            UpgradeRarity.Uncommon => ColorUncommon,
            UpgradeRarity.Rare => ColorRare,
            UpgradeRarity.Legendary => ColorLegendary,
            _ => Color.white
        };
    }
}
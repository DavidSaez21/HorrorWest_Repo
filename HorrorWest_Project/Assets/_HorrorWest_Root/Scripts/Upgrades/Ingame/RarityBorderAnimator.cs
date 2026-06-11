using UnityEngine;
using UnityEngine.UI;

public class RarityBorderAnimator : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Image borderImage;
    [SerializeField] private UpgradeRarity rarity;

    [Header("Velocidad")]
    [SerializeField] private float shimmerSpeed = 1.5f;

    [Header("Colores Common")]
    [SerializeField] private Color[] commonColors = new Color[1];

    [Header("Colores Uncommon")]
    [SerializeField] private Color[] uncommonColors = new Color[6];

    [Header("Colores Rare")]
    [SerializeField] private Color[] rareColors = new Color[6];

    [Header("Colores Legendary")]
    [SerializeField] private Color[] legendaryColors = new Color[6];

    private Color[] _activeColors;
    private float _phase = 0f;

    private void Awake()
    {
        if (borderImage == null)
            borderImage = GetComponent<Image>();

        UpdateActiveColors();
    }

    private void Update()
    {
        if (_activeColors == null || _activeColors.Length < 2) return;

        _phase += Time.unscaledDeltaTime * shimmerSpeed;

        // Cicla entre los 5 colores suavemente
        float t = (_phase % _activeColors.Length);
        int indexA = Mathf.FloorToInt(t) % _activeColors.Length;
        int indexB = (indexA + 1) % _activeColors.Length;
        float blend = t - Mathf.Floor(t);

        borderImage.color = Color.Lerp(_activeColors[indexA], _activeColors[indexB], blend);
    }

    public void SetRarity(UpgradeRarity newRarity)
    {
        rarity = newRarity;
        UpdateActiveColors();
    }

    private void UpdateActiveColors()
    {
        _activeColors = rarity switch
        {
            UpgradeRarity.Common => commonColors,
            UpgradeRarity.Uncommon => uncommonColors,
            UpgradeRarity.Rare => rareColors,
            UpgradeRarity.Legendary => legendaryColors,
            _ => commonColors
        };
    }
}
using UnityEngine;
using UnityEngine.UI;

public class RarityBorderAnimator : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Image borderImage;
    private UpgradeRarity rarity;

    [Header("Velocidad")]
    [SerializeField] private float shimmerSpeed = 1.5f;

    [Header("Color Common (fijo)")]
    [SerializeField] private Color commonColor = Color.gray;

    [Header("Colores Uncommon")]
    [SerializeField] private Color[] uncommonColors = new Color[6];

    [Header("Colores Rare")]
    [SerializeField] private Color[] rareColors = new Color[6];

    [Header("Colores Legendary")]
    [SerializeField] private Color[] legendaryColors = new Color[6];

    private Color[] _activeColors;
    private bool _isStatic = false;
    private float _phase = 0f;

    private void Awake()
    {
        if (borderImage == null)
            borderImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (borderImage == null) return;

        if (_isStatic)
        {
            borderImage.color = commonColor;
            return;
        }

        if (_activeColors == null || _activeColors.Length < 2) return;

        _phase += Time.unscaledDeltaTime * shimmerSpeed;

        float t = _phase % _activeColors.Length;
        int indexA = Mathf.FloorToInt(t) % _activeColors.Length;
        int indexB = (indexA + 1) % _activeColors.Length;
        float blend = t - Mathf.Floor(t);

        borderImage.color = Color.Lerp(_activeColors[indexA], _activeColors[indexB], blend);
    }

    public void SetRarity(UpgradeRarity newRarity)
    {
        rarity = newRarity;

        if (rarity == UpgradeRarity.Common)
        {
            _isStatic = true;
            _activeColors = null;
        }
        else
        {
            _isStatic = false;
            _activeColors = rarity switch
            {
                UpgradeRarity.Uncommon => uncommonColors,
                UpgradeRarity.Rare => rareColors,
                UpgradeRarity.Legendary => legendaryColors,
                _ => uncommonColors
            };
        }
    }
}
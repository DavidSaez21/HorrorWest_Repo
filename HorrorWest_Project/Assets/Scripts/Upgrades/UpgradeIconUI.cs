using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona los iconos de mejoras activas en la esquina superior derecha.
/// Los iconos aparecen de izquierda a derecha y permanecen toda la run.
/// </summary>
public class UpgradeIconUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform iconContainer;       // Panel horizontal donde se añaden los iconos
    [SerializeField] private GameObject iconPrefab;         // Prefab simple con Image

    [Header("Settings")]
    [SerializeField] private float iconSize = 40f;
    [SerializeField] private float iconSpacing = 5f;

    public static UpgradeIconUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Añade un icono al panel. Llamado desde UpgradePanel al seleccionar una mejora.
    /// </summary>
    public void AddIcon(Sprite icon)
    {
        if (iconPrefab == null || iconContainer == null) return;

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(iconSize, iconSize);
        }

        Image img = iconObj.GetComponent<Image>();
        if (img != null && icon != null)
            img.sprite = icon;
    }

    /// <summary>
    /// Limpia todos los iconos al morir (nueva run).
    /// </summary>
    public void ClearIcons()
    {
        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);
    }
}
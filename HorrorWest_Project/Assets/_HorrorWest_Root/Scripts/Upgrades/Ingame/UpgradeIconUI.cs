using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeIconUI : MonoBehaviour
{
    public static UpgradeIconUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject iconPrefab;

    [Header("Settings")]
    [SerializeField] private float iconSize = 40f;

    [Header("Stack Badges")]
    [SerializeField] private Sprite badgeX2;
    [SerializeField] private Sprite badgeX3;
    [SerializeField] private Sprite badgeX4;
    [SerializeField] private Sprite badgeX5;
    [SerializeField] private Sprite badgeX6;

    private Dictionary<UpgradeType, (GameObject iconObj, int count)> activeIcons
        = new Dictionary<UpgradeType, (GameObject, int)>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddIcon(UpgradeType upgradeType, Sprite icon)
    {
        if (iconPrefab == null || iconContainer == null) return;

        if (activeIcons.ContainsKey(upgradeType))
        {
            int newCount = activeIcons[upgradeType].count + 1;
            GameObject existingObj = activeIcons[upgradeType].iconObj;
            activeIcons[upgradeType] = (existingObj, newCount);
            UpdateBadge(existingObj, newCount);
            return;
        }

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(iconSize, iconSize);

        Image img = iconObj.GetComponent<Image>();
        if (img != null && icon != null)
            img.sprite = icon;

        Image badge = iconObj.transform.Find("Badge")?.GetComponent<Image>();
        if (badge != null) badge.gameObject.SetActive(false);

        activeIcons[upgradeType] = (iconObj, 1);
    }

    private void UpdateBadge(GameObject iconObj, int count)
    {
        Image badge = iconObj.transform.Find("Badge")?.GetComponent<Image>();
        if (badge == null) return;

        Sprite badgeSprite = count switch
        {
            2 => badgeX2,
            3 => badgeX3,
            4 => badgeX4,
            5 => badgeX5,
            6 => badgeX6,
            _ => badgeX6    // Si pasa de x6 muestra x6
        };

        if (badgeSprite != null)
        {
            badge.sprite = badgeSprite;
            badge.gameObject.SetActive(true);
        }
    }

    public void ClearIcons()
    {
        foreach (Transform child in iconContainer)
            Destroy(child.gameObject);
        activeIcons.Clear();
    }
}
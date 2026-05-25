using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UpgradeCard[] cards;

    private void Start()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp += ShowPanel;
        else
            Debug.LogError("UpgradePanel: ExperienceManager no encontrado");
    }

    private void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp -= ShowPanel;
    }

    private void ShowPanel(int level)
    {
        if (panelRoot == null) { Debug.LogError("panelRoot es NULL"); return; }

        Time.timeScale = 0f;

        float luck = PlayerStats.Instance != null ? PlayerStats.Instance.GetLuckMultiplier() : 1f;
        List<UpgradeData> upgrades = UpgradePool.Instance.GetRandomUpgrades(3, luck);

        for (int i = 0; i < cards.Length; i++)
        {
            if (i < upgrades.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(upgrades[i], OnUpgradeSelected);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        panelRoot.SetActive(true);
    }

    private void OnUpgradeSelected(UpgradeData upgrade)
    {
        if (upgrade.isUnique)
            UpgradePool.Instance.RegisterUniqueUpgrade(upgrade.upgradeType);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ApplyUpgrade(upgrade);

        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }
}
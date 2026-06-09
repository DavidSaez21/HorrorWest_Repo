using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UpgradeCard[] cards;

    [Header("Settings")]
    [SerializeField] private float inputDelay = 0.8f;

    private void Awake()
    {
        // Busca el panelRoot como hijo directo si la referencia se ha roto
        if (panelRoot == null)
            panelRoot = transform.Find("UpgradePanel")?.gameObject;
    }

    private void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp += ShowPanel;
        else
            Debug.LogError("[UpgradePanel] ExperienceManager no encontrado");
    }

    private void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnLevelUp -= ShowPanel;
    }

    private void ShowPanel(int level)
    {
        if (panelRoot == null) { Debug.LogError("[UpgradePanel] panelRoot es NULL"); return; }

        Time.timeScale = 0f;

        float luck = PlayerManager.Instance?.Stats?.GetLuckMultiplier() ?? 1f;
        List<UpgradeData> upgrades = UpgradePool.Instance.GetRandomUpgrades(3, luck);

        foreach (UpgradeCard card in cards)
            card.SetInteractable(false);

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
        StartCoroutine(EnableCardsAfterDelay());
    }

    private IEnumerator EnableCardsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(inputDelay);
        foreach (UpgradeCard card in cards)
            card.SetInteractable(true);
    }

    private void OnUpgradeSelected(UpgradeData upgrade)
    {
        if (upgrade.isUnique)
            UpgradePool.Instance.RegisterUniqueUpgrade(upgrade.upgradeType);

        PlayerManager.Instance?.ApplyUpgrade(upgrade);

        if (UpgradeIconUI.Instance != null && upgrade.activeIcon != null)
            UpgradeIconUI.Instance.AddIcon(upgrade.upgradeType, upgrade.activeIcon);

        panelRoot.SetActive(false);
        StartCoroutine(ResumeAfterFrame());
    }

    private IEnumerator ResumeAfterFrame()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;
    }
}
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    void Awake()
    {
        Instance = this;
        tooltipPanel.SetActive(false);
    }

    public void ShowTooltip(string title, string description)
    {
        tooltipPanel.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
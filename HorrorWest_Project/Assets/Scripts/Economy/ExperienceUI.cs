using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image xpBarFill;       // La imagen con Fill Method Horizontal
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        if (ExperienceManager.Instance == null)
        {
            Debug.LogError("ExperienceUI: ExperienceManager no encontrado");
            return;
        }

        ExperienceManager.Instance.OnXPChanged += UpdateUI;
        ExperienceManager.Instance.OnLevelUp += OnLevelUp;
        UpdateUI(ExperienceManager.Instance.GetCurrentXP(), ExperienceManager.Instance.GetXPToNextLevel());
    }

    private void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnXPChanged -= UpdateUI;
            ExperienceManager.Instance.OnLevelUp -= OnLevelUp;
        }
    }

    private void UpdateUI(float currentXP, float xpRequired)
    {
        if (xpBarFill == null) return;
        xpBarFill.fillAmount = currentXP / xpRequired;
    }

    private void OnLevelUp(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"Nivel {newLevel}";
    }
}
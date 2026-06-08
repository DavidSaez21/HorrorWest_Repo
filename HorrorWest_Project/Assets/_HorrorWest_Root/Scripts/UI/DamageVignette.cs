using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image vignetteImage;

    [Header("Configuración")]
    [SerializeField] private float activationThreshold = 0.35f; // % de vida para activarse
    [SerializeField] private float minAlpha = 0.1f;             // opacidad mínima al 35%
    [SerializeField] private float maxAlpha = 0.85f;            // opacidad máxima al 0%

    private void Start()
    {
        if (vignetteImage != null)
            SetAlpha(0f);

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnHealthChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        float percent = current / max;

        if (percent >= activationThreshold)
        {
            SetAlpha(0f);
            return;
        }

        // Mapea 0% vida → maxAlpha, 35% vida → minAlpha
        float t = 1f - (percent / activationThreshold);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        SetAlpha(alpha);
    }

    private void SetAlpha(float alpha)
    {
        if (vignetteImage == null) return;
        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }
}
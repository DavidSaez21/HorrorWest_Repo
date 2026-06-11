using System.Collections;
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

    private PlayerHealth _subscribedHealth;

    private void OnEnable()
    {
        StartCoroutine(KeepConnected());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // Vigila si el PlayerHealth cambia (player nuevo en cada run) y se reconecta
    private IEnumerator KeepConnected()
    {
        while (true)
        {
            if (PlayerHealth.Instance != _subscribedHealth)
            {
                Unsubscribe();
                if (PlayerHealth.Instance != null)
                {
                    _subscribedHealth = PlayerHealth.Instance;
                    _subscribedHealth.OnHealthChanged += OnHealthChanged;
                    // Sincroniza con la vida actual del nuevo player
                    OnHealthChanged(_subscribedHealth.GetCurrentHealth(), _subscribedHealth.GetMaxHealth());
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void Unsubscribe()
    {
        if (_subscribedHealth != null)
        {
            _subscribedHealth.OnHealthChanged -= OnHealthChanged;
            _subscribedHealth = null;
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        float percent = current / max;
        if (percent >= activationThreshold)
        {
            SetAlpha(0f);
            return;
        }
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
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar UI for the Church Boss.
///
/// Setup:
///   1. Create a Canvas (Screen Space — Overlay) in the boss scene.
///   2. Add a panel at the bottom of the screen with a background Image.
///   3. Add a child Image (fill method = Filled, Fill Method = Horizontal) → assign to fillImage.
///   4. Optionally add a Text/TMP component for the boss name → assign to bossNameText.
///   5. Assign this component's bossEnemy field to the ChurchBoss GameObject.
///
/// The bar smoothly lerps to the current HP value for a polished feel.
/// It pulses red when the boss is below 30% HP.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private EnemyBase bossEnemy;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;

    [Header("Optional")]
    [SerializeField] private TMPro.TextMeshProUGUI bossNameText;
    [SerializeField] private string bossDisplayName = "La Iglesia";
    [SerializeField] private CanvasGroup canvasGroup; // for fade-in on enter

    [Header("Colors")]
    [SerializeField] private Color healthyColor  = new Color(0.69f, 0.10f, 0.10f); // dark red
    [SerializeField] private Color criticalColor = new Color(1.00f, 0.22f, 0.22f); // bright red
    [SerializeField] private Color emptyColor    = new Color(0.20f, 0.20f, 0.20f);

    [Header("Smooth Fill")]
    [Tooltip("Speed at which the health bar visually catches up to the real HP value.")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Critical Pulse")]
    [Tooltip("HP percentage below which the bar starts pulsing.")]
    [SerializeField] private float criticalThreshold = 0.30f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseMinAlpha = 0.5f;

    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 0.8f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private float _displayedFill = 1f;
    private float _targetFill    = 1f;
    private bool  _isCritical    = false;
    private float _fadeTimer     = 0f;
    private bool  _fadeDone      = false;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        if (bossNameText != null)
            bossNameText.text = bossDisplayName;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            _fadeDone = false;
        }
        else
        {
            _fadeDone = true;
        }
    }

    private void Update()
    {
        // ── Fade in ───────────────────────────────────────────────────────────
        if (!_fadeDone && canvasGroup != null)
        {
            _fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(_fadeTimer / fadeInDuration);
            if (_fadeTimer >= fadeInDuration) _fadeDone = true;
        }

        // ── Read HP from boss ──────────────────────────────────────────────────
        if (bossEnemy != null)
            _targetFill = bossEnemy.GetHealthPercent();

        // ── Smooth fill ───────────────────────────────────────────────────────
        _displayedFill = Mathf.Lerp(_displayedFill, _targetFill, smoothSpeed * Time.deltaTime);
        if (fillImage != null) fillImage.fillAmount = _displayedFill;

        // ── Color & critical pulse ─────────────────────────────────────────────
        _isCritical = _targetFill < criticalThreshold;

        if (fillImage != null)
        {
            if (_isCritical)
            {
                // Pulse between healthyColor and criticalColor
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
                fillImage.color = Color.Lerp(healthyColor, criticalColor, pulse);

                // Also pulse the alpha of the fill slightly
                Color c = fillImage.color;
                c.a = Mathf.Lerp(pulseMinAlpha, 1f, pulse);
                fillImage.color = c;
            }
            else
            {
                fillImage.color = Color.Lerp(emptyColor, healthyColor, _displayedFill);
            }
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call this to reassign the boss reference at runtime if needed.</summary>
    public void SetBoss(EnemyBase boss) => bossEnemy = boss;
}

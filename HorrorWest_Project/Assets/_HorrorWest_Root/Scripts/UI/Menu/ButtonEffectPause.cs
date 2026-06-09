using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEffectPause : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Balanceo al pasar el raton")]
    public float hoverAngle = 12f;
    public float swingSpeed = 2.2f;
    public float decaySpeed = 1.5f;
    [Range(0f, 1f)]
    public float smoothness = 0.92f;

    [Header("Escala al pasar el raton")]
    public float hoverScaleMultiplier = 1.1f;
    public float scaleSpeed = 8f;

    private float _currentAngle = 0f;
    private float _phase = 0f;
    private bool _isHovered = false;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;

    void Start()
    {
        _originalScale = transform.localScale;
        _originalRotation = transform.localRotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
    }

    void Update()
    {
        if (_isHovered)
        {
            _phase += Time.unscaledDeltaTime * swingSpeed;
            float targetAngle = Mathf.Sin(_phase) * hoverAngle;
            _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, 1f - smoothness);
        }
        else
        {
            _currentAngle = Mathf.Lerp(_currentAngle, 0f, Time.unscaledDeltaTime * decaySpeed);
        }

        transform.localRotation = _originalRotation * Quaternion.Euler(0f, 0f, _currentAngle);

        Vector3 targetScale = _isHovered ? _originalScale * hoverScaleMultiplier : _originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    void OnDisable()
    {
        _isHovered = false;
        transform.localScale = _originalScale;
        transform.localRotation = _originalRotation;
    }
}
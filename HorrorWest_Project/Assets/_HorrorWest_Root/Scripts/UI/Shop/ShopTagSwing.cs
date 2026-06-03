using UnityEngine;
using UnityEngine.EventSystems;

public class ShopTagSwing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Balanceo al pasar el ratón")]
    public float swingAngle = 15f;
    public float swingSpeed = 3f;
    public float decaySpeed = 2f;
    public float fadeInSpeed = 3f;

    [Range(0f, 1f)]
    public float smoothness = 0.85f;

    private float _currentAngle = 0f;
    private float _phase = 0f;
    private float _currentAmplitude = 0f;
    private bool _isHovered = false;
    private bool _fadingIn = false;
    private RectTransform _parentRectTransform;

    void Start()
    {
        _parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_fadingIn)
        {
            _currentAmplitude = Mathf.Lerp(_currentAmplitude, swingAngle, Time.unscaledDeltaTime * fadeInSpeed);
            if (_currentAmplitude >= swingAngle * 0.95f)
                _fadingIn = false;
        }
        else
        {
            _currentAmplitude = Mathf.Lerp(_currentAmplitude, 0f, Time.unscaledDeltaTime * decaySpeed);
        }

        if (_currentAmplitude > 0.1f)
        {
            _phase += Time.unscaledDeltaTime * swingSpeed;

            if (_currentAmplitude < 1f && !_isHovered)
            {
                _currentAmplitude = 0f;
                _currentAngle = Mathf.Lerp(_currentAngle, 0f, Time.unscaledDeltaTime * decaySpeed * 3f);
            }
            else
            {
                float targetAngle = Mathf.Sin(_phase) * _currentAmplitude;
                _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, 1f - smoothness);
            }
        }
        else
        {
            _currentAngle = Mathf.Lerp(_currentAngle, 0f, Time.unscaledDeltaTime * decaySpeed);
        }

        _parentRectTransform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _fadingIn = true;

        Vector3 localPos = _parentRectTransform.InverseTransformPoint(eventData.position);
        bool enterFromRight = localPos.x > 0;
        bool currentGoingRight = _currentAngle > 0;

        if (enterFromRight != currentGoingRight)
            _phase = Mathf.PI - _phase;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _fadingIn = false;
    }
}
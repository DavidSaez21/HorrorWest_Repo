using System.Collections;
using UnityEngine;

/// <summary>
/// Simple singleton camera shaker.
/// Attach to the Camera GameObject (or a parent of it).
///
/// Usage from any script:
///   CameraShaker.Instance?.Shake(magnitude, duration);
///
/// magnitude: max offset in world units (0.05–0.3 is the typical range).
/// duration:  seconds the shake lasts.
///
/// Multiple simultaneous shake requests are additive up to a cap.
/// </summary>
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [SerializeField] private float maxMagnitudeCap = 0.5f;

    private Vector3 _originLocalPos;
    private float _currentMagnitude = 0f;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _originLocalPos = transform.localPosition;
    }

    public void Shake(float magnitude, float duration)
    {
        _currentMagnitude = Mathf.Min(_currentMagnitude + magnitude, maxMagnitudeCap);
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration));
    }

    private IEnumerator ShakeRoutine(float duration)
    {
        float elapsed = 0f;
        float startMag = _currentMagnitude;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t   = elapsed / duration;
            float mag = Mathf.Lerp(startMag, 0f, t);

            transform.localPosition = _originLocalPos + (Vector3)Random.insideUnitCircle * mag;
            yield return null;
        }

        transform.localPosition = _originLocalPos;
        _currentMagnitude = 0f;
        _shakeCoroutine = null;
    }
}

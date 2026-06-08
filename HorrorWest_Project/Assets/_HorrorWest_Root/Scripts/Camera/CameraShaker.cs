using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    [SerializeField] private float maxMagnitudeCap = 0.5f;

    private float _currentMagnitude = 0f;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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
            float t = elapsed / duration;
            float mag = Mathf.Lerp(startMag, 0f, t);

            // Offset relativo a la posición actual en lugar de una posición guardada
            Vector3 offset = (Vector3)Random.insideUnitCircle * mag;
            transform.position += offset;

            yield return null;

            // Revertir el offset antes del siguiente frame
            transform.position -= offset;
        }

        _currentMagnitude = 0f;
        _shakeCoroutine = null;
    }
}
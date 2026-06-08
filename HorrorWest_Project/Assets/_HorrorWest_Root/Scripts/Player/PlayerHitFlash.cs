using System.Collections;
using UnityEngine;

public class PlayerHitFlash : MonoBehaviour
{
    [SerializeField] private float flashAlpha = 0.3f;

    private SpriteRenderer[] _renderers;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void StartInvincibilityFlash(float duration)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        SetAlpha(flashAlpha);
        yield return new WaitForSeconds(duration);
        SetAlpha(1f);
        _flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        foreach (var sr in _renderers)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
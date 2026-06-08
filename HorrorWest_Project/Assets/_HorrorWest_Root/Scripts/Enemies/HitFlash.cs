using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Material flashMaterial;  // asigna el material FlashWhite aquí

    private SpriteRenderer[] _renderers;
    private Material[] _originalMaterials;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        _originalMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalMaterials[i] = _renderers[i].material;
    }

    public void Flash()
    {
        if (_renderers == null || _renderers.Length == 0) return;
        if (flashMaterial == null) return;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (var sr in _renderers)
            if (sr != null) sr.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].material = _originalMaterials[i];

        _flashCoroutine = null;
    }
}
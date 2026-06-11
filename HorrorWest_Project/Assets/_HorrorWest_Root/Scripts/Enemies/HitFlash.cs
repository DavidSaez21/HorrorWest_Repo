using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Material flashMaterial;

    private SpriteRenderer[] _renderers;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
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
        // Guarda los materiales actuales justo antes de flashear
        Material[] currentMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            currentMaterials[i] = _renderers[i] != null ? _renderers[i].material : null;

        foreach (var sr in _renderers)
            if (sr != null) sr.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        // Restaura los materiales que había antes del flash
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null && currentMaterials[i] != null)
                _renderers[i].material = currentMaterials[i];

        _flashCoroutine = null;
    }
}
using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Material flashMaterial;

    private SpriteRenderer[] _renderers;
    private Material[] _cleanMaterials;   // materiales limpios guardados UNA vez
    private Coroutine _flashCoroutine;
    private bool _isFlashing = false;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        // Guarda los materiales limpios una sola vez, al inicio
        _cleanMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _cleanMaterials[i] = _renderers[i].material;
    }

    public void Flash()
    {
        if (_renderers == null || _renderers.Length == 0) return;
        if (flashMaterial == null) return;

        // Si ya está flasheando, reinicia el temporizador pero no re-guarda materiales
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        _isFlashing = true;

        foreach (var sr in _renderers)
            if (sr != null) sr.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

        // Restaura SIEMPRE los materiales limpios guardados en Awake
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].material = _cleanMaterials[i];

        _isFlashing = false;
        _flashCoroutine = null;
    }

    // Para que otros efectos (plaga) sepan si está flasheando
    public bool IsFlashing => _isFlashing;
}
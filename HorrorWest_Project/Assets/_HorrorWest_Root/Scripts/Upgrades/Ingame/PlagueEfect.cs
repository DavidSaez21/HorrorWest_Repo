using System.Collections;
using UnityEngine;

public class PlagueEffect : MonoBehaviour
{
    private float _damagePerSecond;
    private float _duration;
    private GameObject _vfxInstance;
    private SpriteRenderer[] _renderers;
    private Material[] _originalMaterials;

    [SerializeField] private GameObject plagueVFXPrefab;
    [SerializeField] private Material plagueMaterial;

    public void Init(float dps, float duration, GameObject vfxPrefab = null, Transform spawnPoint = null, Material mat = null)
    {
        _damagePerSecond = dps;
        _duration = duration;

        if (vfxPrefab != null) plagueVFXPrefab = vfxPrefab;
        if (mat != null) plagueMaterial = mat;

        // Aplica material verde a todos los renderers
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        _originalMaterials = new Material[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _originalMaterials[i] = _renderers[i].material;

        if (plagueMaterial != null)
            foreach (var sr in _renderers)
                if (sr != null) sr.material = plagueMaterial;

        Transform anchor = spawnPoint != null ? spawnPoint : transform;
        if (plagueVFXPrefab != null)
            _vfxInstance = Instantiate(plagueVFXPrefab, anchor.position, Quaternion.identity, anchor);

        StartCoroutine(PlagueRoutine());
    }

    private IEnumerator PlagueRoutine()
    {
        float elapsed = 0f;
        EnemyBase enemy = GetComponent<EnemyBase>();

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            enemy?.TakeDamageSilent(_damagePerSecond * Time.deltaTime);
            yield return null;
        }

        // Restaura materiales originales
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].material = _originalMaterials[i];

        if (_vfxInstance != null)
            Destroy(_vfxInstance);

        Destroy(this);
    }
}
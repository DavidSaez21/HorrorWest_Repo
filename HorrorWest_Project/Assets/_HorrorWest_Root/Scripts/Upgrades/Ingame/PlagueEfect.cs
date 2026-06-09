using System.Collections;
using UnityEngine;

public class PlagueEffect : MonoBehaviour
{
    private float _damagePerSecond;
    private float _duration;
    private GameObject _vfxInstance;
    private Transform _anchor;

    [SerializeField] private GameObject plagueVFXPrefab;

    public void Init(float dps, float duration, GameObject vfxPrefab = null, Transform spawnPoint = null)
    {
        _damagePerSecond = dps;
        _duration = duration;
        _anchor = spawnPoint != null ? spawnPoint : transform;

        if (vfxPrefab != null)
            plagueVFXPrefab = vfxPrefab;

        // Instancia sin padre, en la raíz de la escena
        if (plagueVFXPrefab != null)
            _vfxInstance = Instantiate(plagueVFXPrefab, _anchor.position, Quaternion.identity);

        StartCoroutine(PlagueRoutine());
    }

    private void Update()
    {
        // Sigue al enemigo manualmente
        if (_vfxInstance != null && _anchor != null)
            _vfxInstance.transform.position = _anchor.position;
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

        if (_vfxInstance != null)
            Destroy(_vfxInstance);

        Destroy(this);
    }
}